using System.Collections.Generic;
using System.Linq;
using Vostok.SnoopDog.Core.Utils;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Stats
{
    public class TypeByHeapGensStatsBuilder : HeapStatBuilder<(string Name, int Address)>
    {
        public override IEnumerable<Stat> Build()
        {
            return typesStats
               .GroupBy(e => e.Key.Address)
               .OrderBy(g => g.Key)
               .Select(
                    g =>
                        new TypesByGensStat(
                            $"Types statistics for {(g.Key == 3 ? "LOH" : $"generation {g.Key}")}",
                            "Numbers of objects and total sizes of each type.",
                            g.ToDictionary(e => e.Key.Name, e => e.Value),
                            g.Key));
        }

        protected override (string, int) Descriminator(ClrObject o, ClrRuntime runtime, ClrType type)
            => (type?.Name ?? "Unknown", runtime.GetGenOrLOH(o.Address));
    }
}