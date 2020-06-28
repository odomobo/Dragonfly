using System;
using System.Collections.Generic;
using System.Text;
using SharpHeart.Engine.Interfaces;

namespace SharpHeart.Engine.MoveGen
{
    public sealed class MoveGen : IMoveGen
    {
        private static readonly ulong[] PawnPromotionSourceTable = GeneratePawnPromotionSourceTable();
        
        public void Generate(ref List<Move> moves, Board board)
        {
            GeneratePawnMoves(ref moves, board, board.GetPieceBitboard(PieceType.Pawn, board.SideToMove));
            GenerateKnightMoves(ref moves, board, board.GetPieceBitboard(PieceType.Knight, board.SideToMove));
            // we can combine generation of queen with bishop and rook moves because when we store moves in the move list, we don't record the piece type which is moving
            GenerateBishopMoves(
                ref moves,
                board,
                board.GetPieceBitboard(PieceType.Bishop, board.SideToMove) |
                board.GetPieceBitboard(PieceType.Queen, board.SideToMove)
            );
            GenerateRookMoves(
                ref moves, 
                board, 
                board.GetPieceBitboard(PieceType.Rook, board.SideToMove) | 
                board.GetPieceBitboard(PieceType.Queen, board.SideToMove)
            );
            GenerateKingNormalMoves(
                ref moves,
                board,
                board.GetPieceBitboard(PieceType.King, board.SideToMove)
            );
            GenerateKingCastling(
                ref moves,
                board
            );
        }

        private void GeneratePawnMoves(ref List<Move> moves, Board board, ulong sourceSquares)
        {
            var color = board.SideToMove;
            ulong promotionSourceSquares = sourceSquares & PawnPromotionSourceTable[(int) color];
            ulong nonPromotionSourceSquares = sourceSquares & ~promotionSourceSquares;

            foreach (var sourceIx in Bits.Enumerate(nonPromotionSourceSquares))
            {
                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                var normalCaptures = captures & board.GetOccupied(color.Other());
                var enPassantCaptures = captures & board.EnPassant;

                var quiets = PawnQuietMoveTable.GetMoves(sourceIx, board.GetOccupied(), color);
                // note: no need to mask quiets; the move table already takes care of that

                foreach (var dstIx in Bits.Enumerate(quiets))
                    moves.Add(Move.Make(MoveType.Normal, MoveType.Quiet, sourceIx, dstIx));

                foreach (var dstIx in Bits.Enumerate(normalCaptures))
                    moves.Add(Move.Make(MoveType.Normal, MoveType.Capture, sourceIx, dstIx));

                foreach (var dstIx in Bits.Enumerate(enPassantCaptures))
                    moves.Add(Move.Make(MoveType.EnPassant, MoveType.Capture, sourceIx, dstIx));
            }

            foreach (var sourceIx in Bits.Enumerate(promotionSourceSquares))
            {
                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                captures &= board.GetOccupied(color.Other());

                var quiets = PawnQuietMoveTable.GetMoves(sourceIx, board.GetOccupied(), color);
                // note: no need to mask quiets; the move table already takes care of that

                foreach (var piece in new[] {PieceType.Queen, PieceType.Knight, PieceType.Rook, PieceType.Bishop})
                {
                    foreach (var dstIx in Bits.Enumerate(quiets))
                        moves.Add(Move.Make(MoveType.Promotion, MoveType.Quiet, sourceIx, dstIx, piece));

                    foreach (var dstIx in Bits.Enumerate(captures))
                        moves.Add(Move.Make(MoveType.Promotion, MoveType.Capture, sourceIx, dstIx, piece));
                }
            }
        }

        private void GenerateKnightMoves(ref List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = KnightMoveTable.GetMoves(sourceIx);
                // don't allow knight to move on piece of same color
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                GenerateMoves(ref moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateBishopMoves(ref List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = BishopMoveTable.GetMoves(sourceIx, board.GetOccupied());
                GenerateMoves(ref moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateRookMoves(ref List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = RookMoveTable.GetMoves(sourceIx, board.GetOccupied());
                GenerateMoves(ref moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateKingNormalMoves(ref List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = KingMoveTable.GetMoves(sourceIx);
                // don't allow king to move on piece of same color
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                foreach (var dstIx in Bits.Enumerate(dstSquares))
                {
                    GenerateMoves(ref moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
                }
            }
        }

        private void GenerateKingCastling(ref List<Move> moves, Board board)
        {
            var castlingDsts = board.CastlingRights & CastlingTables.GetCastlingRightsDstColorMask(board.SideToMove);
            foreach (var dstIx in Bits.Enumerate(castlingDsts))
            {
                if ((board.GetOccupied() & CastlingTables.GetCastlingEmptySquares(dstIx)) > 0)
                    continue;

                if (board.InCheck(board.SideToMove))
                    continue;

                bool attacked = false;
                foreach (var potentiallyAttackedIx in Bits.Enumerate(CastlingTables.GetCastlingAttacks(dstIx)))
                {
                    if (IsAttacked(board, potentiallyAttackedIx, board.SideToMove))
                    {
                        attacked = true;
                        break;
                    }
                }

                if (attacked)
                    continue;

                // if we still have castling rights, the inbetween squares aren't occupied, we aren't in check, and we aren't castling through or into check, then we should be good to go!
                int kingIx = Bits.GetLsb(board.GetPieceBitboard(PieceType.King, board.SideToMove));
                moves.Add(Move.Make(MoveType.Castling, MoveType.Quiet, kingIx, dstIx));
            }
        }

        // Note that this doesn't check if a pawn can be taken en passant
        private bool IsAttacked(Board board, int defenderIx, Color defenderColor)
        {
            // check in roughly descending order of power

            // rook and queen
            var potentialRookCaptures = RookMoveTable.GetMoves(defenderIx, board.GetOccupied());
            if ((potentialRookCaptures & board.GetPieceBitboard(PieceType.Rook, defenderColor.Other())) > 0)
                return true;

            if ((potentialRookCaptures & board.GetPieceBitboard(PieceType.Queen, defenderColor.Other())) > 0)
                return true;

            // bishop and queen
            var potentialBishopCaptures = BishopMoveTable.GetMoves(defenderIx, board.GetOccupied());
            if ((potentialBishopCaptures & board.GetPieceBitboard(PieceType.Bishop, defenderColor.Other())) > 0)
                return true;

            if ((potentialBishopCaptures & board.GetPieceBitboard(PieceType.Queen, defenderColor.Other())) > 0)
                return true;

            var potentialKnightCaptures = KnightMoveTable.GetMoves(defenderIx);
            if ((potentialKnightCaptures & board.GetPieceBitboard(PieceType.Knight, defenderColor.Other())) > 0)
                return true;

            var potentialKingCaptures = KingMoveTable.GetMoves(defenderIx);
            if ((potentialKingCaptures & board.GetPieceBitboard(PieceType.King, defenderColor.Other())) > 0)
                return true;

            var potentialPawnCaptures = PawnCaptureMoveTable.GetMoves(defenderIx, defenderColor);
            if ((potentialPawnCaptures & board.GetPieceBitboard(PieceType.Pawn, defenderColor.Other())) > 0)
                return true;

            // we checked all possible attacks by all possible piece types
            return false;
        }

        private static (ulong quiets, ulong captures) SeparateQuietsCaptures(ulong moves, ulong occupancy)
        {
            ulong quiets = moves & ~occupancy;
            ulong captures = moves & ~quiets;
            return (quiets, captures);
        }

        private void GenerateMoves(ref List<Move> moves, MoveType moveType, int sourceIx, ulong dstSquares, ulong occupancy)
        {
            var (quiets, captures) = SeparateQuietsCaptures(dstSquares, occupancy);

            foreach (var dstIx in Bits.Enumerate(quiets))
                moves.Add(Move.Make(moveType, MoveType.Quiet, sourceIx, dstIx));

            foreach (var dstIx in Bits.Enumerate(captures))
                moves.Add(Move.Make(moveType, MoveType.Capture, sourceIx, dstIx));
        }

        private static ulong[] GeneratePawnPromotionSourceTable()
        {
            var ret = new ulong[2];
            foreach (var color in new[] { Color.White, Color.Black })
            {
                ulong promotionSources = 0;

                int promotionRank;
                if (color == Color.White)
                    promotionRank = 6;
                else
                    promotionRank = 1;

                for (int file = 0; file < 8; file++)
                {
                    promotionSources |= Board.ValueFromFileRank(file, promotionRank);
                }

                ret[(int)color] = promotionSources;
            }

            return ret;
        }
    }
}
