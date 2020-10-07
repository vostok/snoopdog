using System;
using System.Linq;
using System.Runtime.InteropServices;
using CommandLine;
using Kontur.SnoopDog.Core;
using Kontur.SnoopDog.Core.Issues;
using Kontur.SnoopDog.Core.Metrics;
using Kontur.SnoopDog.Core.Stats;
using Microsoft.Diagnostics.Runtime;
using Newtonsoft.Json;

namespace Kontur.SnoopDog
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ProcessArguments, DumpArguments>(args)
               .MapResult(
                    (ProcessArguments a) => Run(a),
                    (DumpArguments a) => Run(a),
                    errs => 1);
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

        [Option("dlk", Default = false, HelpText = "Check for deadlocks")]
        public bool Deadlocks
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterDetector(IssueDetectors.DetectDeadLocks);
            }
        }

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
            set => GetDataTarget = () => DataTarget.AttachToProcess(value, 10000, AttachFlag.Passive);
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
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    GetDataTarget = HandleExceptions(() => DataTarget.LoadCrashDump(value));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    GetDataTarget = HandleExceptions(() => DataTarget.LoadCoreDump(value));
                else
                    throw new PlatformNotSupportedException("MacOS is not supported currently.");
            }
        }
    }
}