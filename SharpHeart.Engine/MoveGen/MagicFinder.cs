using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public class MagicFinder
    {
        private readonly Random _random;
        private readonly int _tableIndexBits;
        private readonly int _tableSize;

        public MagicFinder(Random random, int tableIndexBits)
        {
            _random = random;
            _tableIndexBits = tableIndexBits;
            _tableSize = 1 << tableIndexBits;
        }

        public ulong[] FindMagics(ulong[] masks)
        {
            ulong[] magics = new ulong[64];
            for (int i = 0; i < 64; i++)
            {
                magics[i] = FindMagic(masks[i], Bits.PopCount(masks[i]));
            }

            return magics;
        }

        private const int MaxIterations = 100000000;
        private ulong FindMagic(ulong mask, int maskSize)
        {
            if (maskSize > _tableIndexBits)
                throw new Exception($"Can't find magic for mask because mask bits {maskSize} is greater than table index bits {_tableIndexBits}");

            // TODO: use 1 << maskSize instead
            var used = new bool[_tableSize];
            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                var potentialMagic = GetSparseRandomUlong();
                // if we don't get enough bits in the index for full occupancy, then we have little chance of having all keys provide a unique value
                if (Bits.PopCount((ulong)GetTableIndex(mask, potentialMagic)) < maskSize/2) // less than 6???
                    continue;

                // set all to false
                Array.Clear(used, 0, used.Length);

                int permutationCount = 1 << maskSize;
                bool foundCollision = false;
                for (uint permutation = 0; permutation < permutationCount; permutation++)
                {
                    // We can use this to do every combination of bits in mask.
                    // PDEP is expensive on zen architecture, but this code is only called offline, so the cost isn't that important.
                    var occupancy = Bits.ParallelBitDeposit(permutation, mask);
                    var key = GetTableIndex(occupancy, potentialMagic);

                    if (used[key])
                    {
                        foundCollision = true;
                        break;
                    }

                    used[key] = true;
                }
                if (foundCollision)
                    continue;

                // if no key collisions, then we've found a suitable magic
                return potentialMagic;
            }

            throw new Exception($"Could not find bitboard magic in {MaxIterations} iterations!");
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
        private int GetTableIndex(ulong maskedOccupancy, ulong magic)
        {
            return (int)((maskedOccupancy * magic) >> (64 - _tableIndexBits));
        }
    }
}
