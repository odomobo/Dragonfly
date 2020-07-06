using System;

namespace SharpHeart.Engine
{
    [Flags]
    public enum MoveType
    {
        Normal = 1,
        DoubleMove = 2,
        EnPassant = 4,
        Promotion = 8,
        Castling = 16,
        Quiet = 32,
        Capture = 64,
    }

    public class Move
    {
        // Note that we could easily compact this to 2 bytes, and then change Move into a readonly struct
        public readonly MoveType MoveType;
        public readonly PieceType PieceType;
        public readonly int SourceIx;
        public readonly int DstIx;
        public readonly PieceType PromotionPiece;

        public Move(MoveType moveType, PieceType pieceType, int sourceIx, int dstIx, PieceType promotionPiece = PieceType.None)
        {
            MoveType = moveType;
            PieceType = pieceType;
            SourceIx = sourceIx;
            DstIx = dstIx;
            PromotionPiece = promotionPiece;
        }

        public override string ToString()
        {
            return BoardParsing.NaiveSanStringFromMove(this);
        }
    }
}