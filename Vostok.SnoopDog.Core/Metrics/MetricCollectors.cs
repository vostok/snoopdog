using System.Collections.Generic;
using System.Linq;
using Vostok.SnoopDog.Core.Utils;
using Microsoft.Diagnostics.Runtime;
using Vostok.SnoopDog.Core.Stats;

namespace Vostok.SnoopDog.Core.Metrics
{
    public static class MetricCollectors
    {
        public static Metric CollectThreadCountMetric(ClrRuntime runtime) =>
            new Metric("Threads count", runtime.Threads.Count);


        public static IEnumerable<Metric> CollectHeapGenerationMetrics(ClrRuntime runtime, Report report)
        {
            if (report.Stats.Any(s => s is TypesByGensStat))
                return report.Stats
                    .Select(s => s as TypesByGensStat)
                    .Where(s => s != null)
                    .OrderBy(s => s.HeapGen)
                    .Select(
                        s => new Metric(
                            s.HeapGen != 3 ? $"Heap generation {s.HeapGen} objects count" : "Large Objects Heap objects count",
                            s.TypesStats.Sum(kv => kv.Value.Count)));
            
            return runtime.Heap
                .EnumerateObjectAddresses()
                .Select(runtime.GetGenOrLOH)
                .GroupBy(g => g)
                .OrderBy(g => g.Key)
                .Select(g => new Metric(
                    g.Key != 3 ? 
                        $"Heap generation {g.Key} objects count" : 
                        "Large Objects Heap objects count", 
                    g.Count()));
        }
    }
}