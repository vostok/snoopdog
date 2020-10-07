using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Stats
{
    public class TypesStatBuilder : HeapStatBuilder<string>
    {
        protected override string Descriminator(ClrObject clrObject, ClrRuntime runtime, ClrType type)
            => type?.Name;

        public override IEnumerable<Stat> Build()
        {
            yield return new TypesStat(
                "Types statistics",
                "Numbers of objects and total sizes of each type.",
                typesStats);
        }
    }
}