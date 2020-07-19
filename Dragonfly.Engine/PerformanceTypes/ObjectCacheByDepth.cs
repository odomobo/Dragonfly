using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.PerformanceTypes
{
    public sealed class ObjectCacheByDepth<T> where T : new()
    {
        // TODO: use array instead of list?
        private List<T> _cache;

        public ObjectCacheByDepth(int initialSize = 10)
        {
            _cache = new List<T>(initialSize);
            ResizeTo(initialSize);
        }

        public T Get(int ply)
        {
            if (_cache.Count <= ply)
                ResizeTo(ply*2);

            return _cache[ply];
        }

        private void ResizeTo(int newSize)
        {
            while (_cache.Count < newSize)
                _cache.Add(new T());
        }
    }
}
