using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.SnoopDog.Core.Utils;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Stats
{
    public static class StatCollectors
    {
        public static Stat CollectStackTraceStats(ClrRuntime runtime)
        {
            var stackTraceStats = runtime.Threads
               .Select(t => Tuple.Create(t.ManagedThreadId, t.GetStackTraceRepr()))
               .GroupBy(t => t.Item2, t => t.Item1, new StackTraceComparer())
               .ToDictionary(g => g.Key, g => g.ToArray());

            return new StackTraceStat(stackTraceStats);
        }
    }

    internal class StackTraceComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[] x, string[] y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            if (x.Length != y.Length)
                return false;
            for (var i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;
            return true;
        }

        public int GetHashCode(string[] obj) =>
            obj.GetAggregatedHashCode();
    }
}