using System.Collections.Generic;
using Vostok.SnoopDog.Core.Issues;
using Vostok.SnoopDog.Core.Metrics;
using Vostok.SnoopDog.Core.Stats;

namespace Vostok.SnoopDog.Core
{
    public class Report
    {
        public IReadOnlyList<IIssue> Issues { get; internal set; }
        public IReadOnlyList<Metric> Metrics { get; internal set; }
        public IReadOnlyList<Stat> Stats { get; internal set; }
    }
}