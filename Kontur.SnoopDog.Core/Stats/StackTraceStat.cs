using System.Collections.Generic;
using System.Linq;

namespace Kontur.SnoopDog.Core.Stats
{
    public class StackTraceStat : Stat
    {
        public StackTraceInfo[] StackTraceInfos { get; }


        public StackTraceStat(Dictionary<string[], int[]> stackTraceStats) : 
            base("Uniq stack traces", 
                "For each stack trace list of managed thread ids is provided.")
        {
            StackTraceInfos = stackTraceStats
                .Select(kv => new StackTraceInfo()
                {
                    StackTrace = kv.Key,
                    ThreadsCount = kv.Value.Length,
                    ThreadIds = kv.Value
                })
                .ToArray();
        }
    }

    public class StackTraceInfo
    {
        public string[] StackTrace { get; internal set; }
        public int ThreadsCount { get; internal set; }
        public int[] ThreadIds { get; internal set; }

        internal StackTraceInfo()
        {
        }
    }
}