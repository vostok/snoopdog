using Kontur.Utilities;
using Microsoft.Diagnostics.Runtime;

namespace Kontur.SnoopDog.Core.Stats
{
    public class TypeStat
    {
        public TypeStat(ClrType clrObjectType)
        {
            MethodTable = clrObjectType?.MethodTable;
        }

        public ulong? MethodTable { get; }
        public int Count { get; set; }
        public DataSize TotalSize { get; set; }
    }
}