using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    [DebuggerDisplay("{ToDebugString()}")]
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

        public string ToDebugString()
        {
            string pieceTypeStr;
            // TODO: move lookup to board parsing class
            switch (PieceType)
            {
                case PieceType.Pawn:
                    pieceTypeStr = "";
                    break;
                case PieceType.Knight:
                    pieceTypeStr = "N";
                    break;
                case PieceType.Bishop:
                    pieceTypeStr = "B";
                    break;
                case PieceType.Rook:
                    pieceTypeStr = "R";
                    break;
                case PieceType.Queen:
                    pieceTypeStr = "Q";
                    break;
                case PieceType.King:
                    pieceTypeStr = "K";
                    break;
                default:
                    pieceTypeStr = "?";
                    break;
            }

            string captureStr = "";
            if ((MoveType & MoveType.Capture) > 0)
            {
                if (PieceType == PieceType.Pawn)
                    captureStr = BoardParsing.FileStrFromIx(SourceIx);

                captureStr += "x";
            }

            string dstSquareStr = BoardParsing.SquareStrFromIx(DstIx);

            string promotionPieceStr = "";
            // TODO: move to board parsing class
            if ((MoveType & MoveType.Promotion) > 0)
            {
                switch (PieceType)
                {
                    case PieceType.Knight:
                        promotionPieceStr = "N";
                        break;
                    case PieceType.Bishop:
                        promotionPieceStr = "B";
                        break;
                    case PieceType.Rook:
                        promotionPieceStr = "R";
                        break;
                    case PieceType.Queen:
                        promotionPieceStr = "Q";
                        break;
                    default:
                        promotionPieceStr = "?";
                        break;
                }
            }

            return $"{pieceTypeStr}{captureStr}{dstSquareStr}{promotionPieceStr}";
        }

        // TODO
        Board DoMove(Board b)
        {
            throw new NotImplementedException();
        }
    }
}