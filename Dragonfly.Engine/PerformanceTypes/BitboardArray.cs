using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dragonfly.Engine.PerformanceTypes
{
    /// <summary>
    /// Efficient inline ulong[12] with optional bounds checking
    /// </summary>
    internal unsafe struct BitboardArray
    {
        private fixed ulong _fixedBuffer[12];

        public ulong this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckBounds(index);
                return _fixedBuffer[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckBounds(index);
                _fixedBuffer[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBounds(int index)
        {
#if !DISABLE_BOUNDS_CHECKS
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
#endif
        }

        public int Length => 12;
    }
}
