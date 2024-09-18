using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Engine.PerformanceTypes
{
    [System.Runtime.CompilerServices.InlineArray(2)]
    public struct InlineArray2<T>
    {
        private T _element0;
    }

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

    public static class InlineArrayExtensions
    {
        public static int Length<T>(this InlineArray2<T> self)
        {
            return 2;
        }

        public static int Length<T>(this InlineArray12<T> self)
        {
            return 12;
        }

        public static int Length<T>(this InlineArray64<T> self)
        {
            return 64;
        }
    }
}
