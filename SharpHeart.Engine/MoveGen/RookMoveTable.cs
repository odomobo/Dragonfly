using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public static class RookMoveTable
    {
        private const int MaxMaskSize = 12;
        private static readonly MagicMoveTable RookMoveMagicTable;

        static RookMoveTable()
        {
            var (masks, occupancies, moves) = GetAllMasksOccupanciesMoves();
            try
            {
                RookMoveMagicTable = new MagicMoveTable(masks, Magics, occupancies, moves, MaxMaskSize);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Could not generate table using builtin magics; trying to dynamically generate magics.");
                Console.Error.WriteLine(e);
                var magicFinder = new MagicFinder(new Random(0), MaxMaskSize);
                var dynamicMagics = magicFinder.FindMagics(masks);
                RookMoveMagicTable = new MagicMoveTable(masks, dynamicMagics, occupancies, moves, MaxMaskSize);
            }
        }

        public static ulong GetMoves(int ix, ulong occupancy)
        {
            return RookMoveMagicTable.GetMoves(ix, occupancy);
        }

        private static (ulong mask, ulong[] occupancies, ulong[] moves) GetSingleMaskOccupanciesMoves(
            int srcFile,
            int srcRank)
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

            return (mask, occupancies.ToArray(), moves.ToArray());
        }

        private static (ulong[] masks, ulong[][] occupancies, ulong[][] moves) GetAllMasksOccupanciesMoves()
        {
            var masks = new ulong[64];
            var occupancies = new ulong[64][];
            var moves = new ulong[64][];

            foreach (var (file, rank) in Board.GetAllFilesRanks())
            {
                var (mask, singleOccupancies, singleMoves) = GetSingleMaskOccupanciesMoves(file, rank);
                int ix = Board.IxFromFileRank(file, rank);
                masks[ix] = mask;
                occupancies[ix] = singleOccupancies;
                moves[ix] = singleMoves;
            }

            return (masks, occupancies, moves);
        }

        // This can be used to generate the Magics table
        public static void DumpMagics()
        {
            var (masks, _, _) = GetAllMasksOccupanciesMoves();

            var magicFinder = new MagicFinder(new Random(0), MaxMaskSize);
            var magics = magicFinder.FindMagics(masks);

            for (int i = 0; i < magics.Length; i++)
            {
                var magic = magics[i];

                Console.Write($"0x{magic:x16}UL, ");

                if ((i + 1) % 4 == 0)
                    Console.WriteLine();
            }
        }

        // generated with DumpMagics
        private static readonly ulong[] Magics = 
        {
            0xa280004000288010UL, 0x1440001040082001UL, 0x0080050810002000UL, 0x2200120024022040UL,
            0x0200200c17020010UL, 0x020000c218071004UL, 0x5020208402000ac8UL, 0x1200004104248402UL,
            0x4900800280244008UL, 0x8000880840008210UL, 0x2204640400200840UL, 0x2000100208101480UL,
            0x005020011204000aUL, 0x9000250000880100UL, 0x0002004802100294UL, 0x0006000042009c13UL,
            0x004000802141800aUL, 0x0113900130094404UL, 0x08009010a001c209UL, 0x4008282a00040002UL,
            0x0200101c00012800UL, 0x002d902020080124UL, 0x0c04024003008020UL, 0x0000548003094820UL,
            0x4c16002200110088UL, 0x0080080081884800UL, 0x2010084448040004UL, 0x003a804818010080UL,
            0x012802080080220cUL, 0x0209380200411200UL, 0x080a00a120100802UL, 0xa010040824104100UL,
            0x0080230104080080UL, 0x1048200008408051UL, 0x0206200010100104UL, 0x08000400ac100008UL,
            0x2000800840820400UL, 0x1261000829002402UL, 0x0200a04280242802UL, 0x8080050040800128UL,
            0x20008a5000212002UL, 0x8006094001202009UL, 0x1608203804202000UL, 0x8022000c08402020UL,
            0x04009180440d0401UL, 0x0002002040100400UL, 0x80c1020082048200UL, 0x48800080600d4004UL,
            0x804080c00610300cUL, 0x0090014000600140UL, 0x0008003400200820UL, 0x0083800800040210UL,
            0x2550102110020602UL, 0x919a222404800089UL, 0x00428b1201400180UL, 0x0000e37001000048UL,
            0x14008000c021001bUL, 0x0080290010400421UL, 0x4480108004a03a02UL, 0x0004402062005006UL,
            0x0201000410280005UL, 0x0004000082210019UL, 0x0200210804018042UL, 0xa20e882044008102UL,
        };
    }
}
