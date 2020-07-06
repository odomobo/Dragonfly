using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;
using MersenneTwister;

namespace Dragonfly.Engine.MoveGens
{
    public static class RookMoveTable
    {
        internal static readonly MagicMoveTable RookMoveMagicTable;

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

        // RookMoveTable generated with DumpMagics
        private static readonly ulong[] Magics = {
            0x0080002080400210, 0x104001100340A009, 0x0080081000200080, 0x0080100014800800,
            0x1100080003002410, 0x4100028C00180100, 0x2400010804008210, 0x0480002044800100,
            0x0240800080204000, 0x1025400820100042, 0x1412001022008040, 0x4007002429001000,
            0x0009800C01808800, 0x1604011820106400, 0x000C00010804902A, 0x1013802480044100,
            0x0038808000400060, 0x044C250040090080, 0x0810510041042000, 0x4000818010004800,
            0x0000808008006400, 0xA200808014008A00, 0x00000400012A0850, 0x0000060010840049,
            0x8000400080228005, 0x8080200040045000, 0x0005004100142000, 0x0051010900201000,
            0x0484180080040180, 0x20900C0080800200, 0x0040031400501608, 0x0080004200049401,
            0x800020C004800288, 0x0000201000400040, 0x0011200241001900, 0x0080100081800804,
            0x001480C400801800, 0x000C008D05000900, 0x0046081004004142, 0x0000004B02000384,
            0x02B42094C0008000, 0x2130006000424010, 0x0541023020050040, 0x0209042010010008,
            0x30100D0008010010, 0x1006008004008002, 0x0011010802840010, 0x0203000280410002,
            0x0008800421004100, 0x00010B8226004200, 0xA000621200814200, 0x0602801000180080,
            0x0801010408001100, 0x02080400805A0080, 0x800E000443180A00, 0x0460B06402850200,
            0x0221204100800C51, 0x40614001008410A1, 0x0800104009002003, 0x0001000460100029,
            0x1041000228001005, 0x040100020C000801, 0x0000100800861504, 0x8A830000420180A1,
        };
    }
}
