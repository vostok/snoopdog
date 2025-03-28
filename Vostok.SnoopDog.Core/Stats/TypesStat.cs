﻿using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;
using Vostok.SnoopDog.Core.Stats;

namespace Vostok.SnoopDog.Core.Stats
{
    public class TypesStat : Stat
    {
        public IDictionary<string, TypeStat> TypesStats { get; }


        public TypesStat(string title, string description, IDictionary<string, TypeStat> typesStats) :
            base(title, description)
        {
            TypesStats = typesStats;
        }
    }

    internal class TypesByGensStat : TypesStat
    {
        public Generation HeapGen { get; }
        
        public TypesByGensStat(string title, string description, IDictionary<string, TypeStat> typesStats, Generation heapGen)
            : base(title, description, typesStats)
        {
            HeapGen = heapGen;
        }
    }
}