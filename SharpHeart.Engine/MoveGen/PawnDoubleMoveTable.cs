using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    // TODO: simplify magic move tables?
    public static class PawnDoubleMoveTable
    {
        private const int MaxMaskSize = 2;
        private static readonly MagicMoveTable[] DoubleMovesMagicTables;
        
        static PawnDoubleMoveTable()
        {
            DoubleMovesMagicTables = new MagicMoveTable[2];
            
            foreach (var color in new[] {Color.White, Color.Black})
            {
                var (masks, occupancies, moves) = GetAllMasksOccupanciesMoves(color);
                try
                {
                    DoubleMovesMagicTables[(int) color] = new MagicMoveTable(
                        masks,
                        Magics[(int) color],
                        occupancies,
                        moves,
                        MaxMaskSize);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Could not generate table using builtin magics; trying to dynamically generate magics.");
                    Console.Error.WriteLine(e);
                    var magicFinder = new MagicFinder(new Random(0), MaxMaskSize);
                    var dynamicMagics = magicFinder.FindMagics(masks);
                    DoubleMovesMagicTables[(int)color] = new MagicMoveTable(masks, dynamicMagics, occupancies, moves, MaxMaskSize);
                }
            }
        }

        public static ulong GetMoves(int ix, ulong occupancy, Color color)
        {
            return DoubleMovesMagicTables[(int) color].GetMoves(ix, occupancy);
        }

        private static (ulong mask, ulong[] occupancies, ulong[] moves) GetSingleMaskOccupanciesMoves(int srcFile, int srcRank, Color color)
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

            return (mask, occupancies.ToArray(), moves.ToArray());
        }

        private static bool IsStartingRank(int rank, Color color)
        {
            return (color == Color.White && rank == 1) || (color == Color.Black && rank == 6);
        }

        private static (ulong[] masks, ulong[][] occupancies, ulong[][] moves) GetAllMasksOccupanciesMoves(Color color)
        {
            var masks = new ulong[64];
            var occupancies = new ulong[64][];
            var moves = new ulong[64][];

            foreach (var (file, rank) in Board.GetAllFilesRanks())
            {
                var (mask, singleOccupancies, singleMoves) = GetSingleMaskOccupanciesMoves(file, rank, color);
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
            for (int colorInt = 0; colorInt < 2; colorInt++)
            {
                Color color = (Color) colorInt;

                var (masks, _, _) = GetAllMasksOccupanciesMoves(color);

                var magicFinder = new MagicFinder(new Random(0), MaxMaskSize);
                var magics = magicFinder.FindMagics(masks);

                Console.WriteLine("new[] {");
                for (int i = 0; i < magics.Length; i++)
                {
                    var magic = magics[i];

                    Console.Write($"0x{magic:x16}UL, ");

                    if ((i + 1) % 4 == 0)
                        Console.WriteLine();
                }
                Console.WriteLine("},");
            }
        }

        // generated with DumpMagics
        private static readonly ulong[][] Magics = 
        {
            new[] {
                0x01c8204000004010UL, 0x08e8000004010010UL, 0xc060085280010000UL, 0x28088080868d1000UL,
                0xa004020000008218UL, 0x0814020200080802UL, 0x0002000000002800UL, 0x800108020208006aUL,
                0x4002804081002500UL, 0x1400204000040000UL, 0x1400201102440809UL, 0x4200100a01044000UL,
                0x1080084d00200040UL, 0x1018020409000000UL, 0x1000020904008c40UL, 0x4020430086000098UL,
                0xb0010048b4000000UL, 0x2101012180040201UL, 0x208c101224980440UL, 0x8200801020014810UL,
                0x1009008420820810UL, 0x8000220200000400UL, 0x0020000a02040000UL, 0x0800400100085812UL,
                0x18001c2084108109UL, 0x8020001221040100UL, 0x2004040020000000UL, 0x1000006188100108UL,
                0x0000022008220007UL, 0x0088100805020002UL, 0x8080000b02004010UL, 0x0010a84600800120UL,
                0x0000084400440089UL, 0x00800500102cc100UL, 0x0000400860a04080UL, 0x200802c0010a0000UL,
                0x6000022124040020UL, 0x0000080222420040UL, 0x0088000000820800UL, 0x1520a00020209400UL,
                0x0028009000218040UL, 0x0a200a0284004000UL, 0x000c5a6840003001UL, 0xd000008100081001UL,
                0x6000440104008c80UL, 0x094000142c000424UL, 0x800401002001c280UL, 0x100103030a400140UL,
                0x0000862402010091UL, 0x328041a000036844UL, 0x0010001010001922UL, 0x088801000080800cUL,
                0x0c0044000394150cUL, 0x0404420810000402UL, 0x0240010090203241UL, 0x0002181000201801UL,
                0x005040018001c000UL, 0x8000800210240301UL, 0x014814000000c064UL, 0x00800008090a1003UL,
                0x0410402808880240UL, 0x4140000008410008UL, 0x1008041041190000UL, 0x0424810100500402UL,
                },
            new[] {
                0x3000001040900020UL, 0x80005801a9480000UL, 0x0000040024030001UL, 0x0000010116504400UL,
                0x0004380000a0a000UL, 0x1222100821062020UL, 0x2020300000200110UL, 0x8101018050004904UL,
                0x8c20100220000040UL, 0xd002006481400400UL, 0x2020228800081000UL, 0x1201301300000020UL,
                0x5508830200000124UL, 0x0508409030004002UL, 0x8104800000101000UL, 0x0150001002180000UL,
                0x0080220802420000UL, 0x1020c61024400004UL, 0x3060060004040240UL, 0x0011000a10008510UL,
                0x200c002048000000UL, 0x20040c0400500803UL, 0x200240100101a200UL, 0x0060804000004004UL,
                0x1100440048000060UL, 0x8000404088004000UL, 0x1400204000040000UL, 0x1481304001001080UL,
                0x0010040600008000UL, 0x0021130028000020UL, 0x000202100490c340UL, 0x0000068040a24040UL,
                0x0020004222008c00UL, 0x6501272020000400UL, 0x0000005400480810UL, 0x0000000920000000UL,
                0x101000c700780008UL, 0x4000e00420000310UL, 0x4042020202000040UL, 0x1080084d00200040UL,
                0x1040000882000d00UL, 0x0000000240020802UL, 0x040110002081d000UL, 0x2000c00088043000UL,
                0x8070018004008002UL, 0x2000110822826010UL, 0x5208204111000a02UL, 0x1018020409000000UL,
                0x0440801146800090UL, 0x0001038042360c01UL, 0x0381444024140822UL, 0x0020004009100090UL,
                0x5404000008040003UL, 0x0094c00003040001UL, 0x200802c0010a0000UL, 0x4030000009409080UL,
                0x800401002001c280UL, 0x23404000200e3a00UL, 0x0000806050049004UL, 0x0401024020423188UL,
                0x0104008000000400UL, 0x0008101008000c00UL, 0x0010001010001922UL, 0x0c0044000394150cUL,
            },
        };
    }
}
