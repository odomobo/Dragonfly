using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.Engine
{
    [Flags]
    public enum MoveType
    {
        Normal = 2,
        DoubleMove = 4,
        EnPassant = 8,
        Promotion = 16,
        Castling = 32,
        Quiet = 64,
        Capture = 128,
    }

    public readonly struct Move
    {
        public readonly MoveType MoveType;
        public readonly PieceType PieceType;
        public readonly int SourceIx;
        public readonly int DstIx;
        public readonly PieceType PromotionPiece;

        private Move(MoveType moveType, PieceType pieceType, int sourceIx, int dstIx, PieceType promotionPiece = PieceType.None)
        {
            MoveType = moveType;
            PieceType = pieceType;
            SourceIx = sourceIx;
            DstIx = dstIx;
            PromotionPiece = promotionPiece;
        }

        public static Move Make(
            MoveType type,
            MoveType modifier,
            PieceType pieceType,
            int sourceIx,
            int dstIx,
            PieceType promotionPiece = PieceType.None
        )
        {
            return new Move(type|modifier, pieceType, sourceIx, dstIx, promotionPiece);
        }

        public override string ToString()
        {
            return BoardParsing.NaiveSanStringFromMove(this);
        }
    }
}