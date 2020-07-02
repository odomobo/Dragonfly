using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace SharpHeart.Engine
{
    public static class Bits
    {
        public static int GetLsb(ulong value)
        {
            return BitOperations.TrailingZeroCount(value);
        }

        public static ulong RemoveLsb(ulong value)
        {
            return value & (value - 1);
        }

        public static int PopLsb(ref ulong value)
        {
            var ret = GetLsb(value);
            value = RemoveLsb(value);
            return ret;
        }

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
                return NaiveParallelBitDeposit(value, mask);
            }
        }

        // TODO: test this
        private static ulong NaiveParallelBitDeposit(ulong value, ulong mask)
        {
            ulong ret = 0;
            ulong currentSourceBit = 1;
            for (ulong currentDestBit = 1; currentDestBit != 0; currentDestBit <<= 1)
            {
                if ((mask & currentDestBit) == 0)
                    continue;

                if ((value & currentSourceBit) > 0)
                    ret |= currentDestBit;

                currentSourceBit <<= 1;
            }

            return ret;
        }

        // TODO: test this
        private static ulong NaiveParallelBitExtract(ulong value, ulong mask)
        {
            ulong ret = 0;
            ulong currentDestBit = 1;
            for (ulong currentSourceBit = 1; currentSourceBit != 0; currentSourceBit <<= 1)
            {
                if ((mask & currentSourceBit) == 0)
                    continue;

                if ((value & currentSourceBit) > 0)
                    ret |= currentDestBit;

                currentDestBit <<= 1;
            }

            return ret;
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
