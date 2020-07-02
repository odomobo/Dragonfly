using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpHeart.Engine.MoveGen;

namespace SharpHeart.Engine
{
    public static class Debugging
    {
        public static void DumpMagics()
        {
            Console.WriteLine("// RookMoveTable generated with DumpMagics");
            Console.WriteLine("private static readonly ulong[] Magics = {");
            DumpMagicValues(RookMoveTable.RookMoveMagicTable);
            Console.WriteLine("};");


            Console.WriteLine();
            Console.WriteLine("// BishopMoveTable generated with DumpMagics");
            Console.WriteLine("private static readonly ulong[] Magics = {");
            DumpMagicValues(BishopMoveTable.BishopMoveMagicTable);
            Console.WriteLine("};");

            Console.WriteLine();
            Console.WriteLine("// PawnDoubleMoveTable generated with DumpMagics");
            Console.WriteLine("private static readonly ulong[][] Magics = {");
            Console.WriteLine("new ulong[] {");
            DumpMagicValues(PawnDoubleMoveTable.DoubleMovesMagicTables[0]);
            Console.WriteLine("},");
            Console.WriteLine("new ulong[] {");
            DumpMagicValues(PawnDoubleMoveTable.DoubleMovesMagicTables[1]);
            Console.WriteLine("}");
            Console.WriteLine("};");
        }

        private static void DumpMagicValues(MagicMoveTable magicMoveTable)
        {

            var magics = magicMoveTable.GetMagics().ToList();
            for (int i = 0; i < magics.Count; i++)
            {
                Console.Write($"0x{magics[i]:X16}, ");

                if ((i + 1) % 4 == 0)
                    Console.WriteLine();
            }
        }
    }
}
