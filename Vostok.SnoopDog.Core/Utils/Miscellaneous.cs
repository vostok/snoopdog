using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Utils
{
    public static class Miscellaneous
    {
        public static string EscapeNull(this string s) =>
            s ?? "<No representation>";

        public static int GetAggregatedHashCode<T>(this IEnumerable<T> enumerable)
        {
            return enumerable
                .Aggregate(37, 
                    (a, x) => (a * 32027 + x.GetHashCode()) % 32993);
        }
        
        public static string[] GetStackTraceRepr(this ClrThread thread) =>
            thread.EnumerateStackTrace()
                .Select(frame => frame.ToString().EscapeNull())
                .ToArray();

        // ReSharper disable once InconsistentNaming
        public static int GetGenOrLOH(this ClrRuntime runtime, ulong address)
        {
            var segment = runtime.Heap.GetSegmentByAddress(address);
            var g = segment.GetGeneration(address);

            return g switch
            {
                Generation.Generation0 => 0,
                Generation.Generation1 => 1,
                Generation.Generation2 => 2,
                Generation.Pinned => 3,
                Generation.Large => 3,
                _ => -1
            };
        }
    }
}