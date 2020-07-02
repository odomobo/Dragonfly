using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public class MagicFinder
    {
        private readonly MTRandom _random;

        public MagicFinder(MTRandom random)
        {
            _random = random;
        }

        public ulong[] FindMagics(ulong[] masks)
        {
            ulong[] magics = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                magics[i] = FindMagic(masks[i]);
            }

            return magics;
        }

        private const int MaxIterations = 100000000;
        public ulong FindMagic(ulong mask)
        {
            int maskBits = Bits.PopCount(mask);
            var tableSize = 1 << maskBits;

            // TODO: use 1 << maskSize instead
            var used = new bool[tableSize];
            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                var potentialMagic = GetSparseRandomUlong();

                if (!CheckMagic(mask, maskBits, potentialMagic, used))
                    continue;

                // if no key collisions, then we've found a suitable magic
                return potentialMagic;
            }

            throw new Exception($"Could not find bitboard magic in {MaxIterations} iterations!");
        }

        public static bool CheckMagic(ulong mask, ulong magic)
        {
            int maskBits = Bits.PopCount(mask);
            var tableSize = 1 << maskBits;
            var used = new bool[tableSize];
            return CheckMagic(mask, maskBits, magic, used);
        }

        private static bool CheckMagic(ulong mask, int maskBits, ulong magic, bool[] used)
        {
            // set all to false
            Array.Clear(used, 0, used.Length);

            int permutationCount = 1 << maskBits;
            for (uint permutation = 0; permutation < permutationCount; permutation++)
            {
                // We can use this to do every combination of bits in mask.
                // PDEP is expensive on zen architecture, but this code is only called offline, so the cost isn't that important.
                var occupancy = Bits.ParallelBitDeposit(permutation, mask);
                var key = GetTableIndex(occupancy, magic, maskBits);

                if (used[key])
                    return false;

                used[key] = true;
            }

            return true;
        }

        private ulong GetSparseRandomUlong()
        {
            return _random.genrand_int64() & _random.genrand_int64() & _random.genrand_int64();
        }

        // needs to match the method in MagicMoveTable
        private static int GetTableIndex(ulong maskedOccupancy, ulong magic, int maskBits)
        {
            return (int)((maskedOccupancy * magic) >> (64 - maskBits));
        }
    }
}
