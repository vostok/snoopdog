using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Issues
{
    public static class IssueDetectors
    {
        public static IEnumerable<IIssue> DetectUnhandledExceptions(ClrRuntime runtime, Report report)
        {
            return runtime.Threads
                .Where(t => t.CurrentException != null)
                .Select(t => new UnhandledExceptionIssue(t));
        }
    }
}