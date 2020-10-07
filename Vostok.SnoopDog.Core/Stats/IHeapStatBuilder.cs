using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace Vostok.SnoopDog.Core.Stats
{
    public interface IHeapStatBuilder
    {
        void Consume(ClrObject clrObject, ClrRuntime runtime, ulong size, ClrType type);

        IEnumerable<Stat> Build();
    }
}