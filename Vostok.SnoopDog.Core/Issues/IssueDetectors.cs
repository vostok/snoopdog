using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;
using Vostok.SnoopDog.Core.Utils;

namespace Vostok.SnoopDog.Core.Issues
{
    public static class IssueDetectors
    {
        private class ClrThreadEqualityComparer : IEqualityComparer<ClrThread>
        {
            public bool Equals(ClrThread x, ClrThread y) => object.Equals(x, y);
            public int GetHashCode(ClrThread obj) => obj.GetHashCode();
        }

        public static IEnumerable<IIssue> DetectDeadLocks(ClrRuntime runtime, Report report)
        {            
            var blockedThreads = runtime.Heap
                .EnumerateBlockingObjects()
                .Where(o => o.Owner != null && o.Waiters.Count > 0)
                .Select(o => o.Owner);


            var lockCycles = Graphs
                .FindCycles(
                    blockedThreads,
                    thread => thread.BlockingObjects,
                    obj => obj.Owner,
                    new ClrThreadEqualityComparer());

            return lockCycles.Select(c => new DeadLockIssue(c));
        }

        public static IEnumerable<IIssue> DetectUnhandledExceptions(ClrRuntime runtime, Report report)
        {
            return runtime.Threads
                .Where(t => t.CurrentException != null)
                .Select(t => new UnhandledExceptionIssue(t));
        }
    }
}