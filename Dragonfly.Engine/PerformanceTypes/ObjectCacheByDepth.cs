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

        public T Get(int depth)
        {
            if (_cache.Count <= depth)
                ResizeTo(depth*2);

            return _cache[depth];
        }

        private void ResizeTo(int newSize)
        {
            while (_cache.Count < newSize)
                _cache.Add(new T());
        }
    }
}
