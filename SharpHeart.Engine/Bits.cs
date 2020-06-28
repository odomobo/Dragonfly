using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace SharpHeart.Engine
{
    public static class Bits
    {
        public static int GetLsb(ulong value)
        {
            return BitOperations.LeadingZeroCount(value);
        }

        public static int PopLsb(ref ulong value)
        {
            var ret = GetLsb(value);
            value = value & (value - 1);
            return ret;
        }

        public static int PopCount(ulong value)
        {
            return BitOperations.PopCount(value);
        }

        public static ulong ParallelBitDeposit(ulong value, ulong mask)
        {
            // TODO: conditionally change out logic if not supported; this should be optimized at jit compilation
            return Bmi2.X64.ParallelBitDeposit(value, mask);
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
