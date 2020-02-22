using System.Collections.Generic;

namespace Gimic.Interfaces
{
    public interface IMap<TKey, TValue> : IMap, IDictionary<TKey, TValue>
    {
        IMap<TKey, TValue> Parent { get; }
    }

    public interface IMap
    {
        int NestingLevel { get; }
    }
}
