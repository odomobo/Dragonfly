using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public static class RookMoveTable
    {
        private static readonly MagicMoveTable RookMoveMagicTable;

        static RookMoveTable()
        {
            var magicFinder = new MagicFinder(new MTRandom(0));
            MagicMoveTableBuilder builder = new MagicMoveTableBuilder(magicFinder);

            foreach (var (file, rank) in Board.GetAllFilesRanks())
            {
                AddMovesFromSquare(builder, file, rank);
            }

            RookMoveMagicTable = builder.Build();
        }

        public static ulong GetMoves(int ix, ulong occupancy)
        {
            return RookMoveMagicTable.GetMoves(ix, occupancy);
        }

        private static void AddMovesFromSquare(MagicMoveTableBuilder builder, int srcFile, int srcRank)
        {
            ulong mask = 0;
            List<ulong> occupancies = new List<ulong>();
            List<ulong> moves = new List<ulong>();

            // populate mask
            // Note: mask rays don't extend to the edge of the map; this is because if a ray reaches all the way to the edge of the board, that square's occupancy doesn't affect move generation.

            // left
            for (int dstFile = srcFile - 1; dstFile > 0; dstFile--)
                mask |= Board.ValueFromFileRank(dstFile, srcRank);

            // right
            for (int dstFile = srcFile + 1; dstFile < 7; dstFile++)
                mask |= Board.ValueFromFileRank(dstFile, srcRank);

            // down
            for (int dstRank = srcRank - 1; dstRank > 0; dstRank--)
                mask |= Board.ValueFromFileRank(srcFile, dstRank);

            // up
            for (int dstRank = srcRank + 1; dstRank < 7; dstRank++)
                mask |= Board.ValueFromFileRank(srcFile, dstRank);

            int maskBits = Bits.PopCount(mask);
            int maskPermutations = 1 << maskBits;

            for (int permutation = 0; permutation < maskPermutations; permutation++)
            {
                ulong occupancy = Bits.ParallelBitDeposit((ulong) permutation, mask);
                ulong singularMoves = 0;

                // Note: rays do now reach all the way to the edge of the board; we do want to generate moves that would reach the end of the board.

                // left
                for (int dstFile = srcFile - 1; dstFile >= 0; dstFile--)
                {
                    ulong move = Board.ValueFromFileRank(dstFile, srcRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                // right
                for (int dstFile = srcFile + 1; dstFile < 8; dstFile++)
                {
                    ulong move = Board.ValueFromFileRank(dstFile, srcRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                // down
                for (int dstRank = srcRank - 1; dstRank >= 0; dstRank--)
                {
                    ulong move = Board.ValueFromFileRank(srcFile, dstRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                // up
                for (int dstRank = srcRank + 1; dstRank < 8; dstRank++)
                {
                    ulong move = Board.ValueFromFileRank(srcFile, dstRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                occupancies.Add(occupancy);
                moves.Add(singularMoves);
            }

            var ix = Board.IxFromFileRank(srcFile, srcRank);
            var info = new MagicMoveTable.Info
            {
                Magic = Magics.GetOrDefault(ix),
                Mask = mask,
                MaskedOccupancyKeys = occupancies.ToArray(),
                MovesValues = moves.ToArray(),
            };

            builder.Add(ix, info);
        }

        // generated with DumpMagics
        private static readonly ulong[] Magics = 
        {
        };
    }
}
