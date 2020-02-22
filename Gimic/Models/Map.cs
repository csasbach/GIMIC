using Gimic.Interfaces;
using System.Collections.Generic;

namespace Gimic.Models
{
    public class Map<TKey, TValue> : Dictionary<TKey, TValue>, IMap<TKey, TValue>
    {
        public IMap<TKey, TValue> Parent { get; }

        public int NestingLevel { get; } = 0;

        public Map(IMap<TKey, TValue> parent) : base()
        {
            Parent = parent;
            NestingLevel = (parent?.NestingLevel ?? -1) + 1;
        }
    }
}
