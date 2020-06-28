using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public sealed class MagicMoveTable
    {
        private readonly int _tableIndexBits;
        private readonly int _tableSize;

        private struct TableEntry
        {
            public ulong Mask;
            public ulong[] MovesTable;
            public ulong Magic;
        }

        private readonly TableEntry[] _magicTable;

        public MagicMoveTable(ulong[] masks, ulong[] magics, ulong[][] maskedOccupancyKeys, ulong[][] movesValues, int tableIndexBits)
        {
            _tableIndexBits = tableIndexBits;
            _tableSize = 1 << tableIndexBits;

            _magicTable = new TableEntry[64];
            for (int i = 0; i < 64; i++)
            {
                _magicTable[i].Mask = masks[i];
                _magicTable[i].MovesTable = CreateValuesTable(
                    magics[i],
                    maskedOccupancyKeys[i],
                    movesValues[i]
                );
                _magicTable[i].Magic = magics[i];
            }
        }

        public ulong GetMoves(int positionIx, ulong occupancy)
        {
            var magicEntry = _magicTable[positionIx];

            var maskedOccupancy = magicEntry.Mask & occupancy;
            var tableIndex = GetTableIndex(maskedOccupancy, magicEntry.Magic);
            return magicEntry.MovesTable[tableIndex];
        }

        private ulong[] CreateValuesTable(ulong magic, ulong[] maskedOccupancyKeys, ulong[] movesValues)
        {
            // TODO: use 1 << maskSize instead?
            var movesTable = new ulong[_tableSize];
            for (int i = 0; i < maskedOccupancyKeys.Length; i++)
            {
                var index = GetTableIndex(maskedOccupancyKeys[i], magic);

                // if the current index was already populated, that means we had a key collision. Magics are specifically chosen to prevent key collisions
                if (movesTable[index] > 0)
                    throw new Exception("Magic value is invalid for building magic bitboard lookup table!");

                movesTable[index] = movesValues[i];
            }

            return movesTable;
        }

        // needs to match the method in MagicFinder
        // TODO: use 1 << maskSize instead
        private int GetTableIndex(ulong maskedOccupancy, ulong magic)
        {
            return (int) ((maskedOccupancy * magic) >> (64 - _tableIndexBits));
        }
    }
}
