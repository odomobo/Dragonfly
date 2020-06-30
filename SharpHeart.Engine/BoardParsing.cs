using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpHeart.Engine
{
    // TODO: rename to something nicer
    public static class BoardParsing
    {
        public static Board FromFen(string fen)
        {
            ulong[] whitePieceBitboards = new ulong[(int)PieceType.Count];
            ulong[] blackPieceBitboards = new ulong[(int)PieceType.Count];

            var splitFen = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (splitFen.Count < 1)
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            if (splitFen.Count < 2)
                splitFen.Add("w");

            if (splitFen.Count < 3)
                splitFen.Add("-");

            if (splitFen.Count < 4)
                splitFen.Add("-");

            if (splitFen.Count < 5)
                splitFen.Add("0");

            if (splitFen.Count < 6)
                splitFen.Add("1");

            var splitBoardState = splitFen[0].Split('/');
            if (splitBoardState.Length != 8)
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            for (int i = 0; i < 8; i++)
            {
                int rank = 7 - i; // go from 7 to 0 instead of 0 to 7
                int file = 0;

                foreach (var c in splitBoardState[i])
                {
                    if (c >= '1' && c <= '8')
                    {
                        file += c - '0';
                        continue;
                    }

                    if (BoardParsing.TryParsePiece(c, out var pieceType, out var color))
                    {
                        var value = Board.ValueFromFileRank(file, rank);
                        if (color == Color.White)
                            whitePieceBitboards[(int)pieceType] |= value;
                        else
                            blackPieceBitboards[(int)pieceType] |= value;

                        file++;
                        continue;
                    }

                    // couldn't parse the character
                    throw new Exception("Bad FEN string"); // TODO: better error type & message
                }
                if (file != 8)
                    throw new Exception("Bad FEN string"); // TODO: better error type & message
            }

            Color sideToMove;
            if (splitFen[1] == "w")
                sideToMove = Color.White;
            else if (splitFen[1] == "b")
                sideToMove = Color.Black;
            else
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            // TODO: castling rights
            ulong castlingRights = 0;
            // TODO: en passant
            // TODO: move counts

            return new Board(whitePieceBitboards, blackPieceBitboards, sideToMove, castlingRights);
        }

        public static bool TryParsePiece(char originalC, out PieceType pieceType, out Color color)
        {
            var c = originalC;
            if (c >= 'a' && c <= 'z')
            {
                color = Color.Black;
            }
            else
            {
                // lowercase
                c = (char)(c - 'A' + 'a');
                color = Color.White;
            }

            switch (c)
            {
                case 'p':
                    pieceType = PieceType.Pawn;
                    break;
                case 'n':
                    pieceType = PieceType.Knight;
                    break;
                case 'b':
                    pieceType = PieceType.Bishop;
                    break;
                case 'r':
                    pieceType = PieceType.Rook;
                    break;
                case 'q':
                    pieceType = PieceType.Queen;
                    break;
                case 'k':
                    pieceType = PieceType.King;
                    break;
                default:
                    color = Color.White;
                    pieceType = PieceType.None;
                    return false;
            }

            return true;
        }

        public static string FileStrFromIx(int ix)
        {
            var (file, rank) = Board.FileRankFromIx(ix);
            return $"{(char)('a' + file)}";
        }

        public static string SquareStrFromIx(int ix)
        {
            var (file, rank) = Board.FileRankFromIx(ix);
            return $"{(char)('a' + file)}{rank + 1}";
        }

        #region Debugging

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
                    bool occupied = (Board.ValueFromFileRank(file, rank) & bb) > 0;
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
            Dump(move.ToDebugString());
        }

        public static void Dump(IEnumerable<Move> moves)
        {
            var movesList = moves.ToList();
            Dump("");
            Dump($"######## {movesList.Count()} moves ########");
            foreach (var move in movesList)
                Dump(move);
        }

        // TODO: display the board in a less dumb way
        public static void Dump(Board b)
        {
            foreach (var color in new[] {Color.White, Color.Black})
            {
                for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
                {
                    Dump("");
                    Dump($"######## {color} {pieceType} ########");
                    Dump(b.GetPieceBitboard(pieceType, color));
                }

                Dump("");
                Dump($"######## {color} Occupancy ########");
                Dump(b.GetOccupied(color));
            }

            Dump("");
            Dump($"######## Occupancy ########");
            Dump(b.GetOccupied());

            Dump("");
            Dump($"######## En Passant ########");
            Dump(b.EnPassant);

            Dump("");
            Dump($"######## Castling ########");
            Dump(b.CastlingRights);

            Dump("");
            Dump($"Side to move: {b.SideToMove}");
        }

        public static void Dump(string s)
        {
            Debug.Print(s);
            Console.Error.WriteLine(s);
        }

        #endregion
    }
}
