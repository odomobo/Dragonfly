using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public static class BishopMoveTable
    {
        private const int MaxMaskSize = 9;
        private static readonly MagicMoveTable BishopMoveMagicTable;

        static BishopMoveTable()
        {
            var magicFinder = new MagicFinder(new Random(0));
            MagicMoveTableBuilder builder = new MagicMoveTableBuilder(magicFinder, MaxMaskSize);

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
                Magic = Magics[ix],
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
            0x000401591a022008UL, 0x0814020200080802UL, 0x0012010040042c10UL, 0x0000a02002000408UL,
            0x0800164020092003UL, 0x0080220802420000UL, 0x1020c61024400004UL, 0x0011000a10008510UL,
            0x2000820c44808024UL, 0x4000060080140100UL, 0x1400201102440809UL, 0x0000068040a24040UL,
            0x8000220020c00402UL, 0x0020004222008c00UL, 0x4200100a01044000UL, 0x0000005400480810UL,
            0x1000400120080080UL, 0x101000c700780008UL, 0x0401024020423188UL, 0x0424810100500402UL,
            0x0500118112080800UL, 0x0124580140208602UL, 0x8012480078240040UL, 0x80030a0044003104UL,
            0x0920240844080848UL, 0x00c4082220002060UL, 0x8400180200a03100UL, 0x8c20048208008050UL,
            0x2004840000802008UL, 0x1048020800802400UL, 0x001c08e008c02410UL, 0xa000902102052201UL,
            0x8114012000012202UL, 0x00483a80410342c0UL, 0x6404000804008420UL, 0x01002a8080980200UL,
            0x0740020200002081UL, 0x9010401284014420UL, 0x010c002090503412UL, 0x8104940108002020UL,
            0x01000a9010011812UL, 0x200a84c040200420UL, 0x5002002481000200UL, 0x0400220c20204300UL,
            0x800bf08024012b80UL, 0x00040a0208890015UL, 0x6002002008220549UL, 0x010f040030440002UL,
            0x0009010448420000UL, 0x4480404403009200UL, 0x0005c0d040440808UL, 0x2008000106002dc0UL,
            0x0000100182090002UL, 0x2811182109221820UL, 0x0081108020088184UL, 0x0242840800803200UL,
            0x049a002020222492UL, 0x2080410022004210UL, 0x0000411120200800UL, 0xc048040010808030UL,
            0xa00890000084c413UL, 0x0250042700608101UL, 0x10218241808080e0UL, 0x1000448040142c34UL,
        };
    }
}
