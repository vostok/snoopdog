using System.Collections.Generic;
using System.Linq;
using Vostok.SnoopDog.Core.Utils;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Stats
{
    public class TypeByHeapGensStatsBuilder : HeapStatBuilder<(string, int)>
    {
        protected override (string, int) Descriminator(ClrObject o, ClrRuntime runtime, ClrType type)
            => (type?.Name, runtime.GetGenOrLOH(o.Address));

        public override IEnumerable<Stat> Build()
        {
            return typesStats
                .GroupBy(e => e.Key.Item2)
                .OrderBy(g => g.Key)
                .Select(g =>
                    new TypesByGensStat(
                        $"Types statistics for {(g.Key == 3 ? "LOH" : $"generation {g.Key}")}",
                        "Numbers of objects and total sizes of each type.",
                        g.ToDictionary(e => e.Key.Item1, e => e.Value), g.Key));
        }
    }
}