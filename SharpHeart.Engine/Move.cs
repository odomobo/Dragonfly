using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine
{
    [Flags]
    public enum MoveType
    {
        Normal = 1,
        DoublePawnMove = 2, // TODO: actually use this
        EnPassant = 4,
        Promotion = 8,
        Castling = 16,
        Quiet = 32,
        Capture = 64,
    }

    public readonly struct Move
    {
        public readonly MoveType MoveType;
        public readonly int SourceIx;
        public readonly int DstIx;
        public readonly PieceType PromotionPiece;

        private Move(MoveType moveType, int sourceIx, int dstIx, PieceType promotionPiece = PieceType.None)
        {
            MoveType = moveType;
            SourceIx = sourceIx;
            DstIx = dstIx;
            PromotionPiece = promotionPiece;
        }

        public static Move Make(
            MoveType type,
            MoveType modifier,
            int sourceIx,
            int dstIx,
            PieceType promotionPiece = PieceType.None
        )
        {
            return new Move(type|modifier, sourceIx, dstIx, promotionPiece);
        }


        // TODO
        Board DoMove(Board b)
        {
            throw new NotImplementedException();
        }
    }
}