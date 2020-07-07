using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dragonfly.Engine.PerformanceTypes
{
    /// <summary>
    /// Efficient inline Piece[64] with optional bounds checking
    /// </summary>
    internal unsafe struct PieceSquareArray
    {
        private fixed byte _fixedBuffer[64];

        public Piece this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckBounds(index);
                return (Piece)_fixedBuffer[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckBounds(index);
                _fixedBuffer[index] = (byte)value;
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

        public int Length => 64;
    }
}
