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
        Pawn = 1,
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
            MoveType pawn,
            MoveType type,
            MoveType modifier,
            PieceType pieceType,
            int sourceIx,
            int dstIx,
            PieceType promotionPiece = PieceType.None
        )
        {
            return new Move(pawn|type|modifier, pieceType, sourceIx, dstIx, promotionPiece);
        }

        public override string ToString()
        {
            return BoardParsing.NaiveSanStringFromMove(this);
        }

        public Board DoMove(Board b)
        {
            var castlingRights = b.CastlingRights & CastlingTables.GetCastlingUpdateMask(this);
            switch (MoveType)
            {
                case MoveType.Pawn | MoveType.Normal | MoveType.Quiet:
                    return DoPawnNormalQuietMove(b, castlingRights);
                case MoveType.Normal | MoveType.Quiet:
                    return DoNormalQuietMove(b, castlingRights);
                case MoveType.Normal | MoveType.Capture:
                case MoveType.Pawn | MoveType.Normal | MoveType.Capture:
                    return DoNormalCaptureMove(b, castlingRights);
                case MoveType.Pawn | MoveType.DoubleMove | MoveType.Quiet:
                    return DoDoublePawnMove(b, castlingRights);
                case MoveType.Pawn | MoveType.EnPassant | MoveType.Capture:
                    return DoEnPassantMove(b, castlingRights);
                case MoveType.Pawn | MoveType.Promotion | MoveType.Quiet:
                    return DoPromotionQuietMove(b, castlingRights);
                case MoveType.Pawn | MoveType.Promotion | MoveType.Capture:
                    return DoPromotionCaptureMove(b, castlingRights);
                case MoveType.Castling | MoveType.Quiet:
                    return DoCastlingQuietMove(b, castlingRights);
                default:
                    throw new Exception($"Invalid move type: {MoveType}");
            }
        }

        private Board DoPawnNormalQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] |= Board.ValueFromIx(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, true);
        }

        private Board DoNormalQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] |= Board.ValueFromIx(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, false);
        }

        private Board DoNormalCaptureMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied(b.SideToMove.Other()) & Board.ValueFromIx(DstIx)) > 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] |= Board.ValueFromIx(DstIx);

            for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
            {
                pieceBitboards[Board.PieceBitboardIndex(b.SideToMove.Other(), pieceType)] &= ~Board.ValueFromIx(DstIx);
            }

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, true);
        }

        private Board DoDoublePawnMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] |= Board.ValueFromIx(DstIx);

            var enPassant = Board.ValueFromIx((SourceIx + DstIx) / 2);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, enPassant, b, true);
        }

        private Board DoEnPassantMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            var (srcFile, srcRank) = Board.FileRankFromIx(SourceIx);
            var (dstFile, dstRank) = Board.FileRankFromIx(DstIx);
            var capturedPawnIx = Board.IxFromFileRank(dstFile, srcRank);

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);
            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove.Other(), PieceType.Pawn)] & Board.ValueFromIx(capturedPawnIx)) > 0);
            


            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] |= Board.ValueFromIx(DstIx);
            pieceBitboards[Board.PieceBitboardIndex( b.SideToMove.Other(),  PieceType.Pawn)] &= ~Board.ValueFromIx(capturedPawnIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, true);
        }

        private Board DoPromotionQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PromotionPiece)] |= Board.ValueFromIx(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, true);
        }

        private Board DoPromotionCaptureMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied(b.SideToMove.Other()) & Board.ValueFromIx(DstIx)) > 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PromotionPiece)] |= Board.ValueFromIx(DstIx);

            for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
            {
                pieceBitboards[Board.PieceBitboardIndex(b.SideToMove.Other(), pieceType)] &= ~Board.ValueFromIx(DstIx);
            }

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, true);
        }

        private Board DoCastlingQuietMove(Board b, ulong castlingRights)
        {
            var pieceBitboards = b.GetPieceBitboards();

            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] & Board.ValueFromIx(SourceIx)) > 0);
            Debug.Assert((b.GetOccupied() & Board.ValueFromIx(DstIx)) == 0);
            Debug.Assert((b.GetOccupied() & CastlingTables.GetCastlingEmptySquares(DstIx)) == 0);
            Debug.Assert((pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType.Rook)] & CastlingTables.GetCastlingRookSrcValue(DstIx)) > 0);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] &= ~Board.ValueFromIx(SourceIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType)] |= Board.ValueFromIx(DstIx);

            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType.Rook)] &= ~CastlingTables.GetCastlingRookSrcValue(DstIx);
            pieceBitboards[Board.PieceBitboardIndex(b.SideToMove, PieceType.Rook)] |= CastlingTables.GetCastlingRookDstValue(DstIx);

            return new Board(pieceBitboards, b.SideToMove.Other(), castlingRights, 0, b, false);
        }
    }
}