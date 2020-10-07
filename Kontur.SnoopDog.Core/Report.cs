using System.Collections.Generic;
using Kontur.SnoopDog.Core.Issues;
using Kontur.SnoopDog.Core.Metrics;
using Kontur.SnoopDog.Core.Stats;

namespace Kontur.SnoopDog.Core
{
    public class Report
    {
        public IReadOnlyList<IIssue> Issues { get; internal set; }
        public IReadOnlyList<Metric> Metrics { get; internal set; }
        public IReadOnlyList<Stat> Stats { get; internal set; }
    }
}