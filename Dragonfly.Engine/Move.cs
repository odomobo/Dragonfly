using System;

namespace Dragonfly.Engine
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

    public readonly struct Move
    {
        private readonly byte _moveType;
        private readonly sbyte _sourceIx;
        private readonly sbyte _dstIx;
        private readonly byte _promotionPiece;

        public MoveType MoveType => (MoveType)_moveType;
        public int SourceIx => (int)_sourceIx;
        public int DstIx => (int)_dstIx;
        public PieceType PromotionPiece => (PieceType)_promotionPiece;

        // TODO: remove pieceType for params
        public Move(MoveType moveType, PieceType pieceType, int sourceIx, int dstIx, PieceType promotionPiece = PieceType.None)
        {
            _moveType = (byte)moveType;
            _sourceIx = (sbyte)sourceIx;
            _dstIx = (sbyte)dstIx;
            _promotionPiece = (byte)promotionPiece;
        }

        public override string ToString()
        {
            return BoardParsing.NaiveSanStringFromMove(this);
        }
    }
}