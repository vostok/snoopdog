using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Stats
{
    public class BoxedStructStatBuilder : HeapStatBuilder<string>
    {
        protected override string Descriminator(ClrObject clrObject, ClrRuntime runtime, ClrType type)
            => type.IsValueClass ? type?.Name : null;

        public override IEnumerable<Stat> Build()
        {
            yield return new TypesStat(
                "Boxed structs statistics",
                "Numbers of boxed structs and total sizes of each struct type.",
                typesStats);
        }
    }
}