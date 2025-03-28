﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using CommandLine;
using Microsoft.Diagnostics.Runtime;
using Newtonsoft.Json;
using Vostok.SnoopDog;
using Vostok.SnoopDog.Core;
using Vostok.SnoopDog.Core.Issues;
using Vostok.SnoopDog.Core.Metrics;
using Vostok.SnoopDog.Core.Stats;

namespace Vostok.SnoopDog
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ProcessArguments, DumpArguments>(args)
               .MapResult(
                    (ProcessArguments a) => Run(a),
                    (DumpArguments a) => Run(a),
                    errs => errs.Any(x => 
                        !(x is HelpRequestedError || 
                          x is HelpVerbRequestedError || 
                          x is VersionRequestedError)) ? 1 : 0);
        }

        private static int Run(Arguments options)
        {
            Report[] reports;

            using (var dt = options.GetDataTarget())
            {
                reports = dt.ClrVersions
                   .Select(cv => options.Reporter.Report(cv.CreateRuntime()))
                   .ToArray();
            }

            options.WriteReportsOut(reports);

            return 0;
        }
    }

    internal class Arguments
    {
        public Reporter Reporter { get; } = new Reporter();
        public Func<DataTarget> GetDataTarget { get; protected set; }

        public Action<Report[]> WriteReportsOut { get; protected set; } = reports =>
        {
            var jsonSerializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                Converters = {new DataSizeConverter()}
            };
            jsonSerializer.Serialize(Console.Out, reports);
        };

        [Option("ex", Default = false, HelpText = "Check for unhandled exceptions")]
        public bool Exceptions
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterDetector(IssueDetectors.DetectUnhandledExceptions);
            }
        }

        [Option("tc", Default = false, HelpText = "Count threads")]
        public bool ThreadCount
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterMetrics(MetricCollectors.CollectThreadCountMetric);
            }
        }
        
        [Option("tp", Default = false, HelpText = "Inspect thread pool")]
        public bool ThreadPool
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterMultiMetric(MetricCollectors.CollectThreadPoolMetrics);
            }
        }

        [Option("hg", Default = false, HelpText = "Generations counts and sizes")]
        public bool HeapGenerations
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterMultiMetric(MetricCollectors.CollectHeapGenerationMetrics);
            }
        }

        [Option("ts", Default = false, HelpText = "Object counts and sizes by types")]
        public bool TypesStats
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterHeapStatBuilder<TypesStatBuilder>();
            }
        }

        [Option("st", Default = false, HelpText = "Uniq stack traces and respective managed thread ids")]
        public bool StackTraces
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterStat(StatCollectors.CollectStackTraceStats);
            }
        }

        [Option("bxst", Default = false, HelpText = "Boxed structs counts and total sizes by types")]
        public bool Structs
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterHeapStatBuilder<BoxedStructStatBuilder>();
            }
        }

        [Option("hgts", Default = false, HelpText = "Object counts and sizes by types for each heap generation")]
        public bool TypesStatsByHeapGens
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterHeapStatBuilder<TypeByHeapGensStatsBuilder>();
            }
        }

        [Option("html", Default = false, HelpText = "Render report to html instead of json")]
        public bool Html
        {
            get => false;
            set
            {
                if (value)
                    WriteReportsOut = reports => new HtmlRenderer(Console.Out).Render(reports);
            }
        }
    }

    [Verb("proc", HelpText = "Analyse live process")]
    internal class ProcessArguments : Arguments
    {
        [Value(0, HelpText = "Process id", Required = true)]
        public int ProcessId
        {
            get => 0;
            set
            {
                static Func<DataTarget> HandleExceptions(Func<DataTarget> getDataTarget)
                {
                    return () =>
                    {
                        try
                        {
                            return getDataTarget();
                        }
                        catch
                        {
                            throw new ArgumentException("Unable to attach to process. Note that you can't attach to 32-bit from 64-bit and vice versa.");
                        }
                    };
                }
                
                GetDataTarget = HandleExceptions(() => DataTarget.AttachToProcess(value, false));
            }
        }
    }

    [Verb("dump", HelpText = "Analyse dump file")]
    internal class DumpArguments : Arguments
    {
        [Value(0, HelpText = "Dump file path", Required = true)]
        public string DumpFile
        {
            get => null;
            set
            {
                static Func<DataTarget> HandleExceptions(Func<DataTarget> getDump)
                {
                    return () =>
                    {
                        try
                        {
                            return getDump();
                        }
                        catch
                        {
                            throw new ArgumentException("Unable to read dump. Note that Windows dumps can't be read on Linux and vice versa.");
                        }
                    };
                }
                
                GetDataTarget = HandleExceptions(() => DataTarget.LoadDump(value));
            }
        }
    }
}