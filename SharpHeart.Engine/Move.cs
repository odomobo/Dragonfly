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
        Normal = 1,
        DoublePawnMove = 2,
        EnPassant = 4,
        Promotion = 8,
        Castling = 16,
        Quiet = 32,
        Capture = 64,
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
            return BoardParsing.MoveToNaiveSanString(this);
        }

        public Board DoMove(Board b)
        {
            var castlingRights = b.CastlingRights & CastlingTables.GetCastlingUpdateMask(this);
            switch (MoveType)
            {
                case MoveType.Normal | MoveType.Quiet:
                    return DoNormalQuietMove(b, castlingRights);
                case MoveType.Normal | MoveType.Capture:
                    return DoNormalCaptureMove(b, castlingRights);
                case MoveType.DoublePawnMove | MoveType.Quiet:
                    return DoDoublePawnMove(b, castlingRights);
                case MoveType.EnPassant | MoveType.Capture:
                    return DoEnPassantMove(b, castlingRights);
                case MoveType.Promotion | MoveType.Quiet:
                    return DoPromotionQuietMove(b, castlingRights);
                case MoveType.Promotion | MoveType.Capture:
                    return DoPromotionCaptureMove(b, castlingRights);
                case MoveType.Castling | MoveType.Quiet:
                    return DoCastlingQuietMove(b, castlingRights);
                default:
                    throw new Exception($"Invalid move type: {MoveType}");
            }
        }

        private Board DoNormalQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PieceType] |= Board.ValueFromIx(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }

        private Board DoNormalCaptureMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied(b.SideToMove.Other()) & Board.ValueFromIx(DstIx)) > 0);

            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PieceType] |= Board.ValueFromIx(DstIx);

            for (int i = 0; i < (int)PieceType.Count; i++)
            {
                pieceBitboards[(int)b.SideToMove.Other()][i] &= ~Board.ValueFromIx(DstIx);
            }

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }

        private Board DoDoublePawnMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PieceType] |= Board.ValueFromIx(DstIx);

            // TODO: en passant

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }

        private Board DoEnPassantMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            var (srcFile, srcRank) = Board.FileRankFromIx(SourceIx);
            var (dstFile, dstRank) = Board.FileRankFromIx(DstIx);
            var capturedPawnIx = Board.IxFromFileRank(dstFile, srcRank);

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);
            Debug.Assert((pieceBitboards[(int)b.SideToMove.Other()][(int)PieceType.Pawn] & Board.ValueFromIx(SourceIx)) > 0);
            


            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PieceType] |= Board.ValueFromIx(DstIx);
            pieceBitboards[(int) b.SideToMove.Other()][(int) PieceType.Pawn] &= ~Board.ValueFromIx(capturedPawnIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }

        private Board DoPromotionQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PromotionPiece] |= Board.ValueFromIx(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }

        private Board DoPromotionCaptureMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied(b.SideToMove.Other()) & Board.ValueFromIx(DstIx)) > 0);

            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PromotionPiece] |= Board.ValueFromIx(DstIx);

            for (int i = 0; i < (int)PieceType.Count; i++)
            {
                pieceBitboards[(int)b.SideToMove.Other()][i] &= ~Board.ValueFromIx(DstIx);
            }

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }

        private Board DoCastlingQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);
            Debug.Assert((b.GetOccupied() & CastlingTables.GetCastlingEmptySquares(DstIx)) == 0);
            Debug.Assert((pieceBitboards[(int)b.SideToMove][(int)PieceType.Rook] & CastlingTables.GetCastlingRookSrcValue(DstIx)) > 0);

            pieceBitboards[(int)b.SideToMove][(int)PieceType] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[(int)b.SideToMove][(int)PieceType] |= Board.ValueFromIx(DstIx);

            pieceBitboards[(int)b.SideToMove][(int)PieceType.Rook] &= ~CastlingTables.GetCastlingRookSrcValue(DstIx);
            pieceBitboards[(int)b.SideToMove][(int)PieceType.Rook] |= CastlingTables.GetCastlingRookDstValue(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, b);
        }
    }
}