using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.MoveGeneration.Tables;

namespace Dragonfly.Engine
{
    public static class Debugging
    {
        public static void Dump(string s = "")
        {
            Debug.Print(s);
            // TODO: should this be prefixed with something to indicate debug?
            // TODO: should this be stdout or stderr?
            //Console.Error.WriteLine(s);
        }

        /*
         * This dumps a string that looks like:
         *
         *    +---------------+
         *   8|. X . X . X . .|
         *   7|. . X X X . . .|
         *   6|X X X . X X X X|
         *   5|. . X X X . . .|
         *   4|. X . X . X . .|
         *   3|X . . X . . X .|
         *   2|. . . X . . . X|
         *   1|. . . X . . . .|
         *     A B C D E F G H
         *
         */
        public static string GetBitboardStr(ulong bb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("   +---------------+");
            for (int rank = 7; rank >= 0; rank--)
            {
                sb.Append($"  {rank + 1}|");
                for (int file = 0; file < 8; file++)
                {
                    bool occupied = (Position.ValueFromFileRank(file, rank) & bb) > 0;
                    sb.Append(occupied ? 'X' : '.');
                    if (file < 7)
                        sb.Append(" ");
                }
                sb.AppendLine("|");
            }

            sb.Append("    ");
            for (int file = 0; file < 8; file++)
                sb.Append($"{(char)('A' + file)} ");

            return sb.ToString();
        }

        public static void Dump(ulong bb)
        {
            Dump(GetBitboardStr(bb));
        }

        public static void Dump(IEnumerable<ulong> bbs)
        {
            int index = 0;
            foreach (var bb in bbs)
            {
                Dump("");
                Dump($"######## {index++,2} ########");
                Dump(bb);
            }
        }

        public static void Dump(Move move)
        {
            Dump(BoardParsing.NaiveSanStringFromMove(move));
        }

        public static void Dump(IEnumerable<Move> moves)
        {
            var movesList = moves.ToList();
            Dump("");
            Dump($"######## {movesList.Count} moves ########");
            foreach (var move in movesList)
                Dump(move);
        }

        /*
         * This dumps a string that looks like:
         *
         *    +---------------+
         *   8|r . b . . r k .|
         *   7|. . . . n p p p|
         *   6|p . . . p . . .|
         *   5|. . q p P . . .|
         *   4|. . . . . . . .|
         *   3|. . N . . N . .|
         *   2|P P . . . P P P|
         *   1|. . R Q . R K .|
         *     A B C D E F G H
         *
         * - or -
         *
         *    +---------------+
         *   8|♜ . ♝ . . ♜ ♚ .|
         *   7|. . . . ♞ ♟ ♟ ♟|
         *   6|♟ . . . ♟ . . .|
         *   5|. . ♛ ♟ ♙ . . .|
         *   4|. . . . . . . .|
         *   3|. . ♘ . . ♘ . .|
         *   2|♙ ♙ . . . ♙ ♙ ♙|
         *   1|. . ♖ ♕ . ♖ ♔ .|
         *     A B C D E F G H 
         *
         */
        private static string BoardToStr(Position b, bool unicode = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("   +---------------+");
            for (int rank = 7; rank >= 0; rank--)
            {
                sb.Append($"  {rank + 1}|");
                for (int file = 0; file < 8; file++)
                {
                    var (color, pieceType) = b.GetPiece(Position.IxFromFileRank(file, rank));
                    char pieceDisplay = '.';
                    if (pieceType != PieceType.None)
                    {
                        if (unicode)
                            pieceDisplay = BoardParsing.UnicodePieceFromPieceTypeColor(pieceType, color);
                        else
                            pieceDisplay = BoardParsing.LetterFromPieceTypeColor(pieceType, color);
                    }

                    sb.Append(pieceDisplay);
                    if (file < 7)
                        sb.Append(" ");
                }
                sb.AppendLine("|");
            }

            sb.Append("    ");
            for (int file = 0; file < 8; file++)
                sb.Append($"{(char)('A' + file)} ");

            return sb.ToString();
        }

        public static void Dump(Position position, bool unicode = false)
        {
            foreach (var color in new[] { Color.White, Color.Black })
            {
                for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
                {
                    Dump($"{color} {pieceType}:");
                    Dump(position.GetPieceBitboard(color, pieceType));
                }
            }

            Dump();

            Dump(BoardToStr(position, unicode));
            Dump("");
            string enPassant = BoardParsing.SquareStrFromValue(position.EnPassant);
            string castling = BoardParsing.CastlingStrFromValue(position.CastlingRights);
            Dump($"Side to move: {position.SideToMove}; Move#: {position.FullMove}; Castling: {castling}; En Passant: {enPassant}; 50 move counter: {position.FiftyMoveCounter}");
            Dump($"Zobrist hash: 0x{position.ZobristHash:X16}");
            Dump(BoardParsing.FenStringFromBoard(position));
        }

        public static void DumpMagics()
        {
            Dump("// RookMoveTable generated with DumpMagics");
            Dump("private static readonly ulong[] Magics = {");
            DumpMagicValues(RookMoveTable.RookMoveMagicTable);
            Dump("};");


            Dump();
            Dump("// BishopMoveTable generated with DumpMagics");
            Dump("private static readonly ulong[] Magics = {");
            DumpMagicValues(BishopMoveTable.BishopMoveMagicTable);
            Dump("};");

            Dump();
            Dump("// PawnDoubleMoveTable generated with DumpMagics");
            Dump("private static readonly ulong[][] Magics = {");
            Dump("new ulong[] {");
            DumpMagicValues(PawnDoubleMoveTable.DoubleMovesMagicTables[0]);
            Dump("},");
            Dump("new ulong[] {");
            DumpMagicValues(PawnDoubleMoveTable.DoubleMovesMagicTables[1]);
            Dump("}");
            Dump("};");
        }

        private static void DumpMagicValues(MagicMoveTable magicMoveTable)
        {

            var magics = magicMoveTable.GetMagics().ToList();
            for (int i = 0; i < magics.Count; i++)
            {
                Console.Write($"0x{magics[i]:X16}, ");

                if ((i + 1) % 4 == 0)
                    Dump();
            }
        }
    }
}
