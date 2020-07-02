using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using MersenneTwister;

namespace SharpHeart.Engine.MoveGen
{
    internal static class PawnDoubleMoveTable
    {
        internal static readonly MagicMoveTable[] DoubleMovesMagicTables;
        
        static PawnDoubleMoveTable()
        {
            DoubleMovesMagicTables = new MagicMoveTable[2];
            var magicFinder = new MagicFinder(new MTRandom(0));

            foreach (var color in new[] {Color.White, Color.Black})
            {
                MagicMoveTableBuilder builder = new MagicMoveTableBuilder(magicFinder);

                foreach (var (file, rank) in Board.GetAllFilesRanks())
                {
                    AddMovesFromSquare(builder, file, rank, color);
                }

                DoubleMovesMagicTables[(int)color] = builder.Build();
            }
        }

        public static ulong GetMoves(int ix, ulong occupancy, Color color)
        {
            return DoubleMovesMagicTables[(int) color].GetMoves(ix, occupancy);
        }

        private static void AddMovesFromSquare(MagicMoveTableBuilder builder, int srcFile, int srcRank, Color color)
        {
            ulong mask = 0;
            List<ulong> occupancies = new List<ulong>();
            List<ulong> moves = new List<ulong>();

            int direction = color.GetPawnDirection();
            var dstRank = srcRank + (direction * 2);
            var inbetweenRank = srcRank + direction;
            if (IsStartingRank(srcRank, color))
            {
                mask |= Board.ValueFromFileRank(srcFile, inbetweenRank);
                mask |= Board.ValueFromFileRank(srcFile, dstRank);
            }

            int maskBits = Bits.PopCount(mask);
            int maskPermutations = 1 << maskBits;

            for (int permutation = 0; permutation < maskPermutations; permutation++)
            {
                ulong occupancy = Bits.ParallelBitDeposit((ulong) permutation, mask);
                ulong singularMoves = 0;

                // if there are any pieces in the 2 squares we're checking, then 
                if (IsStartingRank(srcRank, color) && occupancy == 0)
                {
                    singularMoves |= Board.ValueFromFileRank(srcFile, dstRank);
                }

                occupancies.Add(occupancy);
                moves.Add(singularMoves);
            }

            var ix = Board.IxFromFileRank(srcFile, srcRank);
            var info = new MagicMoveTable.Info
            {
                Magic = Magics[(int)color].GetOrDefault(ix),
                Mask = mask,
                MaskedOccupancyKeys = occupancies.ToArray(),
                MovesValues = moves.ToArray(),
            };

            builder.Add(ix, info);
        }

        private static bool IsStartingRank(int rank, Color color)
        {
            return (color == Color.White && rank == 1) || (color == Color.Black && rank == 6);
        }

        // PawnDoubleMoveTable generated with DumpMagics
        private static readonly ulong[][] Magics = {
            new ulong[] {
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x8894609000048000, 0x000820C010121200, 0x0000201041084828, 0x0000120C80124E02,
                0x080448040204A400, 0x2814044A04058010, 0x0000050210020020, 0x000820C100000300,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
            },
            new ulong[] {
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x10190020C4800180, 0x0290050224404000, 0x0004080020100020, 0x2090030808354000,
                0x090002404498008A, 0x00240500048220D0, 0x01A4040012014040, 0x0000800000810200,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
            }
        };
    }
}
