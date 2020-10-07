using System;
using System.Collections.Generic;
using Kontur.Utilities;
using Microsoft.Diagnostics.Runtime;

namespace Kontur.SnoopDog.Core.Stats
{
    public abstract class HeapStatBuilder<T> : IHeapStatBuilder
    {
        protected IDictionary<T, TypeStat> typesStats = new Dictionary<T, TypeStat>();

        public void Consume(ClrObject clrObject, ClrRuntime runtime, ulong size, ClrType type)
        {
            var desc = Descriminator(clrObject, runtime, type);

            if (desc == null)
                return;

            if (!typesStats.TryGetValue(desc, out var entry))
            {
                entry = new TypeStat(type);
                typesStats[desc] = entry;
            }
            entry.Count++;
            entry.TotalSize += DataSize.FromBytes((long) size);
        }

        protected abstract T Descriminator(ClrObject clrObject, ClrRuntime runtime, ClrType type);

        public abstract IEnumerable<Stat> Build();
    }
}