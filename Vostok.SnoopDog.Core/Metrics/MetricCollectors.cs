using System.Collections.Generic;
using System.Linq;
using Vostok.SnoopDog.Core.Utils;
using Microsoft.Diagnostics.Runtime;
using Vostok.SnoopDog.Core.Stats;

namespace Vostok.SnoopDog.Core.Metrics
{
    public static class MetricCollectors
    {
        public static Metric CollectThreadCountMetric(ClrRuntime runtime) 
            => new("Threads count", runtime.Threads.Length);

        public static IEnumerable<Metric> CollectThreadPoolMetrics(ClrRuntime runtime, Report report)
        {
            var threadPool = runtime.ThreadPool;
            if (threadPool == null)
                yield break;
            
            yield return new Metric("Thread pool min workers", threadPool.MinThreads);
            yield return new Metric("Thread pool active workers", threadPool.ActiveWorkerThreads);
            yield return new Metric("Thread pool idle workers", threadPool.IdleWorkerThreads);
            yield return new Metric("Thread pool retired workers", threadPool.RetiredWorkerThreads);
        }

        public static IEnumerable<Metric> CollectHeapGenerationMetrics(ClrRuntime runtime, Report report)
        {
            var result = new List<Metric>();
            
            result.Add(new Metric("GC subheaps count", runtime.Heap.SubHeaps.Length));
            result.Add(new Metric("GC segments count", runtime.Heap.Segments.Length));
            result.Add(new Metric("GC heap in server mode", runtime.Heap.IsServer ? 1 : 0));

            if (report.Stats.Any(s => s is TypesByGensStat))
            {
                result.AddRange(
                    report.Stats
                        .Select(s => s as TypesByGensStat)
                        .Where(s => s != null)
                        .OrderBy(s => s.HeapGen)
                        .SelectMany(
                            s =>
                            {
                                var countMetric = new Metric($"Heap generation '{s.HeapGen}' objects count", s.TypesStats.Sum(kv => kv.Value.Count));
                                var sizeMetric = new Metric($"Heap generation '{s.HeapGen}' objects total size", s.TypesStats.Sum(kv => kv.Value.TotalSize.Bytes));
                                return new[] {countMetric, sizeMetric};
                            }));
            }
            else
            {
                result.AddRange(runtime.Heap
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
                    }));
            }

            return result;
        }
    }
}