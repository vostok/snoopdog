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
            new Metric("Threads count", runtime.Threads.Length);

        public static IEnumerable<Metric> CollectHeapGenerationMetrics(ClrRuntime runtime, Report report)
        {
            if (report.Stats.Any(s => s is TypesByGensStat))
                return report.Stats
                    .Select(s => s as TypesByGensStat)
                    .Where(s => s != null)
                    .OrderBy(s => s.HeapGen)
                    .SelectMany(
                        s =>
                        {
                            var countMetric = new Metric($"Heap generation '{s.HeapGen}' objects count", s.TypesStats.Sum(kv => kv.Value.Count));
                            var sizeMetric = new Metric($"Heap generation '{s.HeapGen}' objects total size", s.TypesStats.Sum(kv => kv.Value.TotalSize.Bytes));
                            return new[] {countMetric, sizeMetric};
                        });
            
            return runtime.Heap
                .EnumerateObjects()
                .Where(o => o.Type != null)
                .Select(o => (Size: o.Size, Gen: runtime.GetGeneration(o.Address)))
                .GroupBy(g => g.Gen)
                .OrderBy(g => g.Key)
                .SelectMany(g =>
                {
                    var countMetric = new Metric($"Heap generation '{g.Key}' objects count", g.Count());
                    var sizeMetric = new Metric($"Heap generation '{g.Key}' objects total size", g.Sum(obj => (long) obj.Size));
                    return new[] {countMetric, sizeMetric};
                });
        }
    }
}