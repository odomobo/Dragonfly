using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace SharpHeart.Engine
{
    public sealed class Board
    {
        private readonly ulong[][] _pieceBitboards;
        public Color SideToMove { get; private set; }
        public ulong EnPassant { get; private set; }
        public ulong CastlingRights { get; private set; }

        public Board()
        {
            _pieceBitboards = new ulong[(int)PieceType.Count][];
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                _pieceBitboards[i] = new ulong[2];
            }
        }

        public Board(ulong[] whitePieceBitboards, ulong[] blackPieceBitboards)
        {
            // TODO: clone the arrays so we don't get issues if the caller modifies them
            _pieceBitboards = new ulong[(int)PieceType.Count][];
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                _pieceBitboards[i] = new ulong[2];
                _pieceBitboards[i][(int) Color.Black] = blackPieceBitboards[i];
                _pieceBitboards[i][(int) Color.White] = whitePieceBitboards[i];
            }
        }

        public void CopyFrom(Board b)
        {
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                Array.Copy(b._pieceBitboards[i], _pieceBitboards[i], _pieceBitboards[i].Length);
            }

            // TODO
        }

        public void Clear()
        {
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                Array.Clear(_pieceBitboards[i], 0, _pieceBitboards[i].Length);
            }

            // TODO
        }

        public ulong GetPieceBitboard(PieceType pt, Color c)
        {
            return _pieceBitboards[(int) pt][(int) c];
        }

        public ulong GetOccupied(Color c)
        {
            // TODO: calculate on board creation?
            return GetPieceBitboard(PieceType.Pawn, c) |
                   GetPieceBitboard(PieceType.Bishop, c) |
                   GetPieceBitboard(PieceType.Knight, c) |
                   GetPieceBitboard(PieceType.Rook, c) |
                   GetPieceBitboard(PieceType.Queen, c) |
                   GetPieceBitboard(PieceType.King, c);
        }

        public ulong GetOccupied()
        {
            return GetOccupied(Color.Black) | GetOccupied(Color.White);
        }

        public bool InCheck(Color side)
        {
            throw new NotImplementedException();
        }

        public PieceType GetPieceType(int ix)
        {
            throw new NotImplementedException();
        }

        // TODO history and stuff?

        #region Static methods

        public static int IxFromFileRank(int file, int rank)
        {
            return rank*8 + file;
        }

        public static bool FileRankOnBoard(int file, int rank)
        {
            return file >= 0 && file < 8 && rank >= 0 && rank < 8;
        }

        public static ulong ValueFromIx(int ix)
        {
            return 1UL << ix;
        }

        public static ulong ValueFromFileRank(int file, int rank)
        {
            return ValueFromIx(IxFromFileRank(file, rank));
        }

        public static IEnumerable<(int file, int rank)> GetAllFilesRanks()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    yield return (file, rank);
                }
            }
        }

        #endregion

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

        // TODO: probably remove this, or replace with something that dumps to interface
        public static void DumpBitboard(ulong bb)
        {
            Console.Error.WriteLine(GetBitboardStr(bb));
        }

        public static void DumpBitboardDebug(ulong bb)
        {
            Debug.Print(GetBitboardStr(bb));
        }

        public static void DumpBitboardsDebug(IEnumerable<ulong> bbs)
        {
            int index = 0;
            foreach (var bb in bbs)
            {
                Debug.Print("");
                Debug.Print($"######## {index++,2} ########");
                DumpBitboardDebug(bb);
            }
        }

        #endregion
    }
}
