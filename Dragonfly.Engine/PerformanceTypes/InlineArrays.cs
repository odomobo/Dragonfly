using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Engine.PerformanceTypes
{
    [System.Runtime.CompilerServices.InlineArray(12)]
    public struct InlineArray12<T>
    {
        private T _element0;
    }

    [System.Runtime.CompilerServices.InlineArray(64)]
    public struct InlineArray64<T>
    {
        private T _element0;
    }

    [System.Runtime.CompilerServices.InlineArray(256)]
    public struct InlineArray256<T>
    {
        private T _element0;
    }

    public struct StaticList256<T> : IEnumerable<T>
    {
        public int Count { get; private set; }
        private InlineArray256<T> _array;

        public void Add(T element)
        {
            _array[Count] = element;
            Count++;
        }

        public void QuickRemoveAt(int i)
        {
            if (i >= Count) throw new IndexOutOfRangeException();

            Count--;

            _array[i] = _array[Count];
            _array[Count] = default;
        }

        public delegate void SpanAction(Span<T> span);

        public void WithSpan(SpanAction action)
        {
            var arraySpan = (Span <T>)_array;
            arraySpan = arraySpan.Slice(0, Count);
            action(arraySpan);
        }

        public delegate U SpanFunction<U>(Span<T> span);

        public U WithSpan<U>(SpanFunction<U> function)
        {
            var arraySpan = (Span<T>)_array;
            arraySpan = arraySpan.Slice(0, Count);
            return function(arraySpan);
        }

        public void Sort()
        {
            WithSpan(s => s.Sort());

            // This might be more efficient? who knows haha
            //var arraySpan = (Span<T>)_array;
            //arraySpan = arraySpan.Slice(0, Count);
            //arraySpan.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            WithSpan(s => s.Sort(comparison));
        }

        public void Sort(int start, int count, IComparer<T> comparison)
        {
            WithSpan(s => s.Slice(start, count).Sort(comparison));
        }

        public T this[int i]
        {
            get { 
                if (i >= Count)
                    throw new IndexOutOfRangeException();
                
                return _array[i]; 
            }

            set {
                if (i >= Count)
                    throw new IndexOutOfRangeException();

                _array[i] = value; 
            }
        }

        private IEnumerable<T> InnerEnumerable()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _array[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InnerEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class InlineArrayExtensions
    {
        public static int Length<T>(this InlineArray12<T> self)
        {
            return 12;
        }

        public static int Length<T>(this InlineArray64<T> self)
        {
            return 64;
        }

        public static int Length<T>(this InlineArray256<T> self)
        {
            return 256;
        }
    }
}
