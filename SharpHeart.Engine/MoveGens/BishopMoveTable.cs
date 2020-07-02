using System;
using System.Collections.Generic;
using System.Text;
using MersenneTwister;

namespace SharpHeart.Engine.MoveGens
{
    public static class BishopMoveTable
    {
        internal static readonly MagicMoveTable BishopMoveMagicTable;

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

        // BishopMoveTable generated with DumpMagics
        private static readonly ulong[] Magics = {
            0x0804A80208012100, 0x8910041802822008, 0x0510140084204010, 0x02380E0040014020,
            0x00240C2200824010, 0x4022111008800300, 0x0201089804400501, 0x10005C0202022000,
            0x0028200415C20400, 0x4420884188048109, 0x8A00100088B10008, 0x8401080A41002280,
            0x1420820210814040, 0x8A00011008AC0080, 0x4016040503103020, 0x0180090092100201,
            0x0804842020820600, 0x48A002020A040700, 0x0002004902040100, 0x0164204802410013,
            0x3004000080A00400, 0x400E000100412420, 0x00008006C2282080, 0x2000A09101082218,
            0x0021300408020840, 0x0002282006108400, 0x23098800D0002024, 0x0010040200401020,
            0x0224082004002020, 0x8501020028480400, 0x000880A001080820, 0x4002005080230800,
            0x0006202000442882, 0x08041A2028920400, 0x0040403000080C40, 0x1810220180080080,
            0x01024100400400C0, 0x0041010600050444, 0x0898008C10B08208, 0x0001410028810400,
            0x30110C8A40022000, 0xB10208B008023441, 0x800102402A001000, 0x0400044208000080,
            0x0000110324000200, 0x0004010045000200, 0xB0D0020800400320, 0x00100A16004000A5,
            0x020C008608204069, 0x0054411801104008, 0x00300100A2900180, 0x0080400284241018,
            0x0084092020444050, 0x0020400204044000, 0x4004A508080100A0, 0x0002080208860226,
            0x2081010050020809, 0x0040020202050402, 0x482010004A180400, 0x0838001021C20220,
            0x4000044014208210, 0x6300400408101100, 0x0406102001040082, 0x0002201101020081,
        };
    }
}
