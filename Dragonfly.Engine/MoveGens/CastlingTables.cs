using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.MoveGens
{
    public static class CastlingTables
    {
        // note that attacks lists don't need to include the king's position; we will be explicitly checking if the king is in check

        private static readonly ulong[] CastlingUpdateMaskTable;
        private static readonly ulong[] CastlingAttacks;
        private static readonly ulong[] CastlingEmptySquares;

        private static readonly int WhiteKingIx = Board.IxFromFileRank(4, 0);
        private static readonly int WhiteKingsideRookIx = Board.IxFromFileRank(7, 0);
        private static readonly int WhiteKingsideDstIx = Board.IxFromFileRank(6, 0);
        private static readonly int WhiteKingsideRookDstIx = Board.IxFromFileRank(5, 0);
        public static readonly ulong WhiteKingsideDst = Board.ValueFromFileRank(6, 0);
        private static readonly ulong WhiteKingsideAttacks = Board.ValueFromFileRank(5, 0) | Board.ValueFromFileRank(6, 0);
        private static readonly ulong WhiteKingsideEmptySquares = Board.ValueFromFileRank(5, 0) | Board.ValueFromFileRank(6, 0);

        private static readonly int WhiteQueensideRookIx = Board.IxFromFileRank(0, 0);
        private static readonly int WhiteQueensideDstIx = Board.IxFromFileRank(2, 0);
        private static readonly int WhiteQueensideRookDstIx = Board.IxFromFileRank(3, 0);
        public static readonly ulong WhiteQueensideDst = Board.ValueFromFileRank(2, 0);
        private static readonly ulong WhiteQueensideAttacks = Board.ValueFromFileRank(3, 0) | Board.ValueFromFileRank(2, 0);
        private static readonly ulong WhiteQueensideEmptySquares = Board.ValueFromFileRank(3, 0) | Board.ValueFromFileRank(2, 0) | Board.ValueFromFileRank(1, 0);

        private static readonly int BlackKingIx = Board.IxFromFileRank(4, 7);
        private static readonly int BlackKingsideRookIx = Board.IxFromFileRank(7, 7);
        private static readonly int BlackKingsideDstIx = Board.IxFromFileRank(6, 7);
        private static readonly int BlackKingsideRookDstIx = Board.IxFromFileRank(5, 7);
        public static readonly ulong BlackKingsideDst = Board.ValueFromFileRank(6, 7);
        private static readonly ulong BlackKingsideAttacks = Board.ValueFromFileRank(5, 7) | Board.ValueFromFileRank(6, 7);
        private static readonly ulong BlackKingsideEmptySquares = Board.ValueFromFileRank(5, 7) | Board.ValueFromFileRank(6, 7);

        private static readonly int BlackQueensideRookIx = Board.IxFromFileRank(0, 7);
        private static readonly int BlackQueensideDstIx = Board.IxFromFileRank(2, 7);
        private static readonly int BlackQueensideRookDstIx = Board.IxFromFileRank(3, 7);
        public static readonly ulong BlackQueensideDst = Board.ValueFromFileRank(2, 7);
        private static readonly ulong BlackQueensideAttacks = Board.ValueFromFileRank(3, 7) | Board.ValueFromFileRank(2, 7);
        private static readonly ulong BlackQueensideEmptySquares = Board.ValueFromFileRank(3, 7) | Board.ValueFromFileRank(2, 7) | Board.ValueFromFileRank(1, 7);

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

        private static readonly ulong[] CastlingRookSrcValues = GenerateCastlingRookSrc();
        private static readonly ulong[] CastlingRookDstValues = GenerateCastlingRookDst();

        private static ulong[] GenerateCastlingRookSrc()
        {
            var ret = new ulong[64];
            ret[WhiteKingsideDstIx] = Board.ValueFromIx(WhiteKingsideRookIx);
            ret[WhiteQueensideDstIx] = Board.ValueFromIx(WhiteQueensideRookIx);
            ret[BlackKingsideDstIx] = Board.ValueFromIx(BlackKingsideRookIx);
            ret[BlackQueensideDstIx] = Board.ValueFromIx(BlackQueensideRookIx);
            return ret;
        }

        private static ulong[] GenerateCastlingRookDst()
        {
            var ret = new ulong[64];
            ret[WhiteKingsideDstIx] = Board.ValueFromIx(WhiteKingsideRookDstIx);
            ret[WhiteQueensideDstIx] = Board.ValueFromIx(WhiteQueensideRookDstIx);
            ret[BlackKingsideDstIx] = Board.ValueFromIx(BlackKingsideRookDstIx);
            ret[BlackQueensideDstIx] = Board.ValueFromIx(BlackQueensideRookDstIx);
            return ret;
        }

        public static ulong GetCastlingRookSrcValue(int castlingDstIx)
        {
            return CastlingRookSrcValues[castlingDstIx];
        }

        public static ulong GetCastlingRookDstValue(int castlingDstIx)
        {
            return CastlingRookDstValues[castlingDstIx];
        }

        #endregion
    }
}
