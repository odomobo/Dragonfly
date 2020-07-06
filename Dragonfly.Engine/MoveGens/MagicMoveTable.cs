using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dragonfly.Engine.MoveGens
{
    public class MagicMoveTable
    {
        // TODO: better name
        public sealed class Info
        {
            public ulong Mask;
            public ulong Magic;
            public ulong[] MaskedOccupancyKeys; 
            public ulong[] MovesValues;
        }

        private struct TableEntry
        {
            public ulong Mask;
            public int MaskBits;
            public ulong[] MovesTable;
            public ulong Magic;
        }

        private readonly TableEntry[] _magicTable;

        public MagicMoveTable(Info[] infos)
        {
            _magicTable = new TableEntry[64];
            for (int i = 0; i < 64; i++)
            {
                _magicTable[i].Mask = infos[i].Mask;

                int maskBits = Bits.PopCount(infos[i].Mask);
                _magicTable[i].MaskBits = maskBits;

                _magicTable[i].MovesTable = CreateValuesTable(infos[i]);
                _magicTable[i].Magic = infos[i].Magic;
            }
        }

        public ulong GetMoves(int positionIx, ulong occupancy)
        {
            var magicEntry = _magicTable[positionIx];

            var maskedOccupancy = magicEntry.Mask & occupancy;
            var tableIndex = GetTableIndex(maskedOccupancy, magicEntry.MaskBits, magicEntry.Magic);
            return magicEntry.MovesTable[tableIndex];
        }

        private ulong[] CreateValuesTable(Info info)
        {
            int maskBits = Bits.PopCount(info.Mask);
            int tableSize = 1 << maskBits;
            var movesTable = new ulong[tableSize];
            for (int i = 0; i < info.MaskedOccupancyKeys.Length; i++)
            {
                var index = GetTableIndex(info.MaskedOccupancyKeys[i], maskBits, info.Magic);

                // if the current index was already populated, that means we had a key collision. Magics are specifically chosen to prevent key collisions
                if (movesTable[index] > 0)
                    throw new Exception("Magic value is invalid for building magic bitboard lookup table!");

                movesTable[index] = info.MovesValues[i];
            }

            return movesTable;
        }

        // needs to match the method in MagicFinder
        private int GetTableIndex(ulong maskedOccupancy, int maskBits, ulong magic)
        {
            return (int) ((maskedOccupancy * magic) >> (64 - maskBits));
        }

        public ulong[] GetMagics()
        {
            return _magicTable.Select(x => x.Magic).ToArray();
        }
    }
}
