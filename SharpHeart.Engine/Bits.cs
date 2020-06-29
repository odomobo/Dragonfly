using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace SharpHeart.Engine
{
    public static class Bits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLsb(ulong value)
        {
            return BitOperations.TrailingZeroCount(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopLsb(ref ulong value)
        {
            var ret = GetLsb(value);
            value = value & (value - 1);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(ulong value)
        {
            return BitOperations.PopCount(value);
        }

        public static ulong ParallelBitDeposit(ulong value, ulong mask)
        {
            if (Bmi2.X64.IsSupported)
            {
                return Bmi2.X64.ParallelBitDeposit(value, mask);
            }
            else
            {
                // TODO: conditionally change out logic if not supported
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<int> Enumerate(ulong value)
        {
            while (value > 0)
            {
                var ix = Bits.PopLsb(ref value);

                yield return ix;
            }
        }
    }
}
