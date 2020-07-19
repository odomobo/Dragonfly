using System;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.MoveGeneration.Tables
{
    public static class CastlingTables
    {
        // note that attacks lists don't need to include the king's position; we will be explicitly checking if the king is in check

        private static readonly ulong[] CastlingUpdateMaskTable;
        private static readonly ulong[] CastlingAttacks;
        private static readonly ulong[] CastlingEmptySquares;

        private static readonly int WhiteKingIx = Position.IxFromFileRank(4, 0);
        private static readonly int WhiteKingsideRookIx = Position.IxFromFileRank(7, 0);
        private static readonly int WhiteKingsideDstIx = Position.IxFromFileRank(6, 0);
        private static readonly int WhiteKingsideRookDstIx = Position.IxFromFileRank(5, 0);
        public static readonly ulong WhiteKingsideDst = Position.ValueFromFileRank(6, 0);
        private static readonly ulong WhiteKingsideAttacks = Position.ValueFromFileRank(5, 0);
        private static readonly ulong WhiteKingsideEmptySquares = Position.ValueFromFileRank(5, 0) | Position.ValueFromFileRank(6, 0);

        private static readonly int WhiteQueensideRookIx = Position.IxFromFileRank(0, 0);
        private static readonly int WhiteQueensideDstIx = Position.IxFromFileRank(2, 0);
        private static readonly int WhiteQueensideRookDstIx = Position.IxFromFileRank(3, 0);
        public static readonly ulong WhiteQueensideDst = Position.ValueFromFileRank(2, 0);
        private static readonly ulong WhiteQueensideAttacks = Position.ValueFromFileRank(3, 0);
        private static readonly ulong WhiteQueensideEmptySquares = Position.ValueFromFileRank(3, 0) | Position.ValueFromFileRank(2, 0) | Position.ValueFromFileRank(1, 0);

        private static readonly int BlackKingIx = Position.IxFromFileRank(4, 7);
        private static readonly int BlackKingsideRookIx = Position.IxFromFileRank(7, 7);
        private static readonly int BlackKingsideDstIx = Position.IxFromFileRank(6, 7);
        private static readonly int BlackKingsideRookDstIx = Position.IxFromFileRank(5, 7);
        public static readonly ulong BlackKingsideDst = Position.ValueFromFileRank(6, 7);
        private static readonly ulong BlackKingsideAttacks = Position.ValueFromFileRank(5, 7);
        private static readonly ulong BlackKingsideEmptySquares = Position.ValueFromFileRank(5, 7) | Position.ValueFromFileRank(6, 7);

        private static readonly int BlackQueensideRookIx = Position.IxFromFileRank(0, 7);
        private static readonly int BlackQueensideDstIx = Position.IxFromFileRank(2, 7);
        private static readonly int BlackQueensideRookDstIx = Position.IxFromFileRank(3, 7);
        public static readonly ulong BlackQueensideDst = Position.ValueFromFileRank(2, 7);
        private static readonly ulong BlackQueensideAttacks = Position.ValueFromFileRank(3, 7);
        private static readonly ulong BlackQueensideEmptySquares = Position.ValueFromFileRank(3, 7) | Position.ValueFromFileRank(2, 7) | Position.ValueFromFileRank(1, 7);

        static CastlingTables()
        {
            CastlingUpdateMaskTable = GenerateCastlingUpdateMaskTable();
            CastlingAttacks = GenerateCastlingAttacks();
            CastlingEmptySquares = GenerateCastlingEmptySquares();
        }

        public static ulong GetCastlingUpdateMask(Move move)
        {
            return CastlingUpdateMaskTable[move.SourceIx] & CastlingUpdateMaskTable[move.DstIx];
        }

        public static ulong GetCastlingAttacks(int dstIx)
        {
            return CastlingAttacks[dstIx];
        }

        public static ulong GetCastlingEmptySquares(int dstIx)
        {
            return CastlingEmptySquares[dstIx];
        }

        public static ulong GetCastlingRightsDstColorMask(Color color)
        {
            switch (color)
            {
                case Color.White:
                    return WhiteKingsideDst | WhiteQueensideDst;
                case Color.Black:
                    return BlackKingsideDst | BlackQueensideDst;
                default:
                    throw new Exception();
            }
        }

        private static ulong[] GenerateCastlingUpdateMaskTable()
        {
            var ret = new ulong[64];
            // start with the assumption that no moves will modify the castling update mask
            for (int ix = 0; ix < 64; ix++)
            {
                ret[ix] = WhiteKingsideDst | WhiteQueensideDst | BlackKingsideDst | BlackQueensideDst;
            }

            // then add the moves that will modify it
            ret[WhiteKingIx] &= ~WhiteKingsideDst & ~WhiteQueensideDst;
            ret[WhiteKingsideRookIx] &= ~WhiteKingsideDst;
            ret[WhiteQueensideRookIx] &= ~WhiteQueensideDst;

            ret[BlackKingIx] &= ~BlackKingsideDst & ~BlackQueensideDst;
            ret[BlackKingsideRookIx] &= ~BlackKingsideDst;
            ret[BlackQueensideRookIx] &= ~BlackQueensideDst;

            return ret;
        }

        private static ulong[] GenerateCastlingAttacks()
        {
            var ret = new ulong[64];
            ret[WhiteKingsideDstIx] = WhiteKingsideAttacks;
            ret[WhiteQueensideDstIx] = WhiteQueensideAttacks;
            ret[BlackKingsideDstIx] = BlackKingsideAttacks;
            ret[BlackQueensideDstIx] = BlackQueensideAttacks;
            return ret;
        }

        private static ulong[] GenerateCastlingEmptySquares()
        {
            var ret = new ulong[64];
            ret[WhiteKingsideDstIx] = WhiteKingsideEmptySquares;
            ret[WhiteQueensideDstIx] = WhiteQueensideEmptySquares;
            ret[BlackKingsideDstIx] = BlackKingsideEmptySquares;
            ret[BlackQueensideDstIx] = BlackQueensideEmptySquares;
            return ret;
        }

        #region DoMove helpers

        private static readonly int[] CastlingRookSrcIxs = GenerateCastlingRookSrcIxs();
        private static readonly int[] CastlingRookDstIxs = GenerateCastlingRookDstIxs();

        private static int[] GenerateCastlingRookSrcIxs()
        {
            var ret = new int[64];
            ret[WhiteKingsideDstIx] = WhiteKingsideRookIx;
            ret[WhiteQueensideDstIx] = WhiteQueensideRookIx;
            ret[BlackKingsideDstIx] = BlackKingsideRookIx;
            ret[BlackQueensideDstIx] = BlackQueensideRookIx;
            return ret;
        }

        private static int[] GenerateCastlingRookDstIxs()
        {
            var ret = new int[64];
            ret[WhiteKingsideDstIx] = WhiteKingsideRookDstIx;
            ret[WhiteQueensideDstIx] = WhiteQueensideRookDstIx;
            ret[BlackKingsideDstIx] = BlackKingsideRookDstIx;
            ret[BlackQueensideDstIx] = BlackQueensideRookDstIx;
            return ret;
        }

        public static int GetCastlingRookSrcIx(int castlingDstIx)
        {
            return CastlingRookSrcIxs[castlingDstIx];
        }

        public static int GetCastlingRookDstIx(int castlingDstIx)
        {
            return CastlingRookDstIxs[castlingDstIx];
        }

        #endregion
    }
}
