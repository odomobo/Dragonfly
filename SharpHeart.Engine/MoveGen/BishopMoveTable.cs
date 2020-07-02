using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public static class BishopMoveTable
    {
        private static readonly MagicMoveTable BishopMoveMagicTable;

        static BishopMoveTable()
        {
            var magicFinder = new MagicFinder(new MTRandom(0));
            MagicMoveTableBuilder builder = new MagicMoveTableBuilder(magicFinder);

            foreach (var (file, rank) in Board.GetAllFilesRanks())
            {
                AddMovesFromSquare(builder, file, rank);
            }

            BishopMoveMagicTable = builder.Build();
        }

        public static ulong GetMoves(int ix, ulong occupancy)
        {
            return BishopMoveMagicTable.GetMoves(ix, occupancy);
        }

        private static void AddMovesFromSquare(MagicMoveTableBuilder builder, int srcFile, int srcRank)
        {
            ulong mask = 0;
            List<ulong> occupancies = new List<ulong>();
            List<ulong> moves = new List<ulong>();

            // populate mask
            // Note: mask rays don't extend to the edge of the map; this is because if a ray reaches all the way to the edge of the board, that square's occupancy doesn't affect move generation.

            // up/left
            for (int i = 1;; i++)
            {
                int dstFile = srcFile - i;
                int dstRank = srcRank + i;
                if (!InsideOuterRing(dstFile, dstRank))
                    break;

                mask |= Board.ValueFromFileRank(dstFile, dstRank);
            }

            // up/right
            for (int i = 1; ; i++)
            {
                int dstFile = srcFile + i;
                int dstRank = srcRank + i;
                if (!InsideOuterRing(dstFile, dstRank))
                    break;

                mask |= Board.ValueFromFileRank(dstFile, dstRank);
            }

            // down/left
            for (int i = 1; ; i++)
            {
                int dstFile = srcFile - i;
                int dstRank = srcRank - i;
                if (!InsideOuterRing(dstFile, dstRank))
                    break;

                mask |= Board.ValueFromFileRank(dstFile, dstRank);
            }

            // down/right
            for (int i = 1; ; i++)
            {
                int dstFile = srcFile + i;
                int dstRank = srcRank - i;
                if (!InsideOuterRing(dstFile, dstRank))
                    break;

                mask |= Board.ValueFromFileRank(dstFile, dstRank);
            }

            int maskBits = Bits.PopCount(mask);
            int maskPermutations = 1 << maskBits;

            for (int permutation = 0; permutation < maskPermutations; permutation++)
            {
                ulong occupancy = Bits.ParallelBitDeposit((ulong)permutation, mask);
                ulong singularMoves = 0;

                // Note: rays do now reach all the way to the edge of the board; we do want to generate moves that would reach the end of the board.

                // up/left
                for (int i = 1; ; i++)
                {
                    int dstFile = srcFile - i;
                    int dstRank = srcRank + i;
                    if (!Board.FileRankOnBoard(dstFile, dstRank))
                        break;

                    ulong move = Board.ValueFromFileRank(dstFile, dstRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                // up/right
                for (int i = 1; ; i++)
                {
                    int dstFile = srcFile + i;
                    int dstRank = srcRank + i;
                    if (!Board.FileRankOnBoard(dstFile, dstRank))
                        break;

                    ulong move = Board.ValueFromFileRank(dstFile, dstRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                // down/left
                for (int i = 1; ; i++)
                {
                    int dstFile = srcFile - i;
                    int dstRank = srcRank - i;
                    if (!Board.FileRankOnBoard(dstFile, dstRank))
                        break;

                    ulong move = Board.ValueFromFileRank(dstFile, dstRank);
                    singularMoves |= move;
                    if ((occupancy & move) > 0)
                        break;
                }

                // down/right
                for (int i = 1; ; i++)
                {
                    int dstFile = srcFile + i;
                    int dstRank = srcRank - i;
                    if (!Board.FileRankOnBoard(dstFile, dstRank))
                        break;

                    ulong move = Board.ValueFromFileRank(dstFile, dstRank);
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

        private static bool InsideOuterRing(int file, int rank)
        {
            return file > 0 && file < 7 && rank > 0 && rank < 7;
        }

        // generated with DumpMagics
        private static readonly ulong[] Magics = 
        {
        };
    }
}
