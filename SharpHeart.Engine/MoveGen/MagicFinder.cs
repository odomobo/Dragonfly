using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public class MagicFinder
    {
        private readonly Random _random;

        public MagicFinder(Random random)
        {
            _random = random;
        }

        public ulong[] FindMagics(ulong[] masks, int tableIndexBits)
        {
            ulong[] magics = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                magics[i] = FindMagic(masks[i], tableIndexBits);
            }

            return magics;
        }

        private const int MaxIterations = 100000000;
        public ulong FindMagic(ulong mask, int tableIndexBits)
        {
            int maskSize = Bits.PopCount(mask);
            if (maskSize > tableIndexBits)
                throw new Exception($"Can't find magic for mask because mask bits {maskSize} is greater than table index bits {tableIndexBits}");

            var tableSize = 1 << tableIndexBits;

            // TODO: use 1 << maskSize instead
            var used = new bool[tableSize];
            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                var potentialMagic = GetSparseRandomUlong();
                // if we don't get enough bits in the index for full occupancy, then we have little chance of having all keys provide a unique value
                if (Bits.PopCount((ulong)GetTableIndex(mask, potentialMagic, tableIndexBits)) < maskSize/2) // less than 6???
                    continue;

                if (!CheckMagic(mask, maskSize, potentialMagic, tableIndexBits, used))
                    continue;

                // if no key collisions, then we've found a suitable magic
                return potentialMagic;
            }

            throw new Exception($"Could not find bitboard magic in {MaxIterations} iterations!");
        }

        public static bool CheckMagic(ulong mask, ulong magic, int tableIndexBits)
        {
            int maskSize = Bits.PopCount(mask);
            if (maskSize > tableIndexBits)
                throw new Exception($"Can't check magic for mask because mask bits {maskSize} is greater than table index bits {tableIndexBits}");

            var tableSize = 1 << tableIndexBits;
            var used = new bool[tableSize];
            return CheckMagic(mask, maskSize, magic, tableIndexBits, used);
        }

        private static bool CheckMagic(ulong mask, int maskSize, ulong magic, int tableIndexBits, bool[] used)
        {
            // set all to false
            Array.Clear(used, 0, used.Length);

            int permutationCount = 1 << maskSize;
            for (uint permutation = 0; permutation < permutationCount; permutation++)
            {
                // We can use this to do every combination of bits in mask.
                // PDEP is expensive on zen architecture, but this code is only called offline, so the cost isn't that important.
                var occupancy = Bits.ParallelBitDeposit(permutation, mask);
                var key = GetTableIndex(occupancy, magic, tableIndexBits);

                if (used[key])
                    return false;

                used[key] = true;
            }

            return true;
        }

        private ulong GetRandomUlong()
        {
            ulong ret = (ulong)_random.Next(1 << 16);
            ret = (ret << 16) | (ulong)_random.Next(1 << 16);
            ret = (ret << 16) | (ulong)_random.Next(1 << 16);
            ret = (ret << 16) | (ulong)_random.Next(1 << 16);
            return ret;
        }

        private ulong GetSparseRandomUlong()
        {
            return GetRandomUlong() & GetRandomUlong() & GetRandomUlong();
        }

        // needs to match the method in MagicMoveTable
        // TODO: use 1 << maskSize instead
        private static int GetTableIndex(ulong maskedOccupancy, ulong magic, int tableIndexBits)
        {
            return (int)((maskedOccupancy * magic) >> (64 - tableIndexBits));
        }
    }
}
