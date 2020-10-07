using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kontur.SnoopDog.Core.Issues;
using Kontur.SnoopDog.Core.Metrics;
using Kontur.SnoopDog.Core.Stats;
using Microsoft.Diagnostics.Runtime;

namespace Kontur.SnoopDog.Core
{
    public class Reporter
    {
        private HashSet<Func<ClrRuntime, Metric>> MetricCollectors { get; } =
            new HashSet<Func<ClrRuntime, Metric>>();

        private HashSet<Func<ClrRuntime, Report, IEnumerable<Metric>>> MultiMetricCollectors { get; } =
            new HashSet<Func<ClrRuntime, Report, IEnumerable<Metric>>>();

        private HashSet<Func<ClrRuntime, Report, IEnumerable<IIssue>>> Detectors { get; } =
            new HashSet<Func<ClrRuntime, Report, IEnumerable<IIssue>>>();

        private HashSet<Func<ClrRuntime, Stat>> StatCollectors { get; } =
            new HashSet<Func<ClrRuntime, Stat>>();

        private HashSet<Func<ClrRuntime, IEnumerable<Stat>>> MultiStatCollectors { get; } =
            new HashSet<Func<ClrRuntime, IEnumerable<Stat>>>();

        private List<IHeapStatBuilder> HeapStatBuilders { get; } = new List<IHeapStatBuilder>();

        public Report Report(ClrRuntime runtime)
        {
            var report = new Report();

            report.Stats = StatCollectors
                .Select(s => s(runtime))
                .Concat(
                    MultiStatCollectors
                        .SelectMany(c => c(runtime)))
                .Concat(BuildHeapStats(runtime))
                .ToList();

            report.Metrics = MetricCollectors
                .Select(m => m(runtime))
                .Concat(
                    MultiMetricCollectors
                        .SelectMany(c => c(runtime, report)))
                .ToList();

            report.Issues = Detectors.SelectMany(d => d(runtime, report)).ToList();

            return report;
        }

        private IEnumerable<Stat> BuildHeapStats(ClrRuntime runtime)
        {
            foreach (var clrObject in runtime.Heap.EnumerateObjects())
            {
                var type = clrObject.Type;
                if (type == null)
                    continue;
                var size = clrObject.Size;
                
                foreach (var builder in HeapStatBuilders)
                    builder.Consume(clrObject, runtime, size, type);
            }

            return HeapStatBuilders
                .SelectMany(builder => builder.Build());
        }

        public Reporter RegisterMetric(Func<ClrRuntime, Metric> metricCollector)
        {
            MetricCollectors.Add(metricCollector);

            return this;
        }

        public Reporter RegisterMultiMetric(Func<ClrRuntime, Report, IEnumerable<Metric>> multiMetricCollector)
        {
            MultiMetricCollectors.Add(multiMetricCollector);

            return this;
        }

        public Reporter RegisterDetector(Func<ClrRuntime, Report, IEnumerable<IIssue>> detector)
        {
            Detectors.Add(detector);

            return this;
        }

        public Reporter RegisterMetrics(params Func<ClrRuntime, Metric>[] metricCollectors)
        {
            foreach (var metricCollector in metricCollectors)
                RegisterMetric(metricCollector);
            return this;
        }

        public Reporter RegisterDetectors(params Func<ClrRuntime, Report, IEnumerable<IIssue>>[] detectors)
        {
            foreach (var detector in detectors)
                RegisterDetector(detector);
            return this;
        }

        public Reporter RegisterStat(Func<ClrRuntime, Stat> statCollector)
        {
            StatCollectors.Add(statCollector);
            return this;
        }

        public Reporter RegisterMultiStat(Func<ClrRuntime, IEnumerable<Stat>> statCollectors)
        {
            MultiStatCollectors.Add(statCollectors);
            return this;
        }

        public Reporter RegisterHeapStatBuilder<T>()
            where T : IHeapStatBuilder, new()
        {
            HeapStatBuilders.Add(new T());
            return this;
        }
    }
}