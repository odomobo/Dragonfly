using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration.Tables;

namespace Dragonfly.Engine.MoveGeneration
{
    public sealed class MoveGenerator : IMoveGenerator
    {
        private static readonly ulong[] PawnPromotionSourceTable = GeneratePawnPromotionSourceTable();

        public bool OnlyLegalMoves => false;

        public void Generate(List<Move> moves, Position position)
        {
            Debug.Assert(!moves.Any());

            GeneratePawnMoves(moves, position, position.GetPieceBitboard(position.SideToMove, PieceType.Pawn));
            GenerateKnightMoves(moves, position, position.GetPieceBitboard(position.SideToMove, PieceType.Knight));
            // we can combine generation of queen with bishop and rook moves because when we store moves in the move list, we don't record the piece type which is moving
            GenerateBishopMoves(
                moves,
                position,
                PieceType.Bishop,
                position.GetPieceBitboard(position.SideToMove, PieceType.Bishop)
            );
            GenerateBishopMoves(
                moves,
                position,
                PieceType.Queen,
                position.GetPieceBitboard(position.SideToMove, PieceType.Queen)
            );
            GenerateRookMoves(
                moves, 
                position, 
                PieceType.Rook,
                position.GetPieceBitboard(position.SideToMove, PieceType.Rook)
            );
            GenerateRookMoves(
                moves,
                position,
                PieceType.Queen,
                position.GetPieceBitboard(position.SideToMove, PieceType.Queen)
            );
            GenerateKingNormalMoves(
                moves,
                position,
                position.GetPieceBitboard(position.SideToMove, PieceType.King)
            );
            GenerateKingCastling(
                moves,
                position
            );
        }

        private void GeneratePawnMoves(List<Move> moves, Position position, ulong sourceSquares)
        {
            var color = position.SideToMove;
            ulong promotionSourceSquares = sourceSquares & PawnPromotionSourceTable[(int) color];
            ulong nonPromotionSourceSquares = sourceSquares & ~promotionSourceSquares;

            //foreach (var sourceIx in Bits.Enumerate(nonPromotionSourceSquares))
            while (Bits.TryPopLsb(ref nonPromotionSourceSquares, out var sourceIx))
            {
                // TODO: separate out normal moves from double moves. Normal moves use normal lookup table, double moves use magic lookup table
                var quiets = PawnQuietMoveTable.GetMoves(sourceIx, color);
                quiets &= ~position.GetOccupied();

                var doubles = PawnDoubleMoveTable.GetMoves(sourceIx, position.GetOccupied(), color);
                // note: no need to mask double moves; the move table already takes care of that

                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                var normalCaptures = captures & position.GetOccupied(color.Other());
                var enPassantCaptures = captures & position.EnPassant;

                //foreach (var dstIx in Bits.Enumerate(quiets))
                while (Bits.TryPopLsb(ref quiets, out var dstIx))
                {
                    moves.Add(new Move(MoveType.Normal|MoveType.Quiet, sourceIx, dstIx));
                }

                //foreach (var dstIx in Bits.Enumerate(doubles))
                while (Bits.TryPopLsb(ref doubles, out var dstIx))
                {
                    moves.Add(new Move(MoveType.DoubleMove|MoveType.Quiet, sourceIx, dstIx));
                }

                //foreach (var dstIx in Bits.Enumerate(normalCaptures))
                while (Bits.TryPopLsb(ref normalCaptures, out var dstIx))
                {
                    moves.Add(new Move(MoveType.Normal|MoveType.Capture, sourceIx, dstIx));
                }

                //foreach (var dstIx in Bits.Enumerate(enPassantCaptures))
                while (Bits.TryPopLsb(ref enPassantCaptures, out var dstIx))
                {
                    moves.Add(new Move(MoveType.EnPassant|MoveType.Capture, sourceIx, dstIx));
                }
            }

            //foreach (var sourceIx in Bits.Enumerate(promotionSourceSquares))
            while (Bits.TryPopLsb(ref promotionSourceSquares, out var sourceIx))
            {
                var quiets = PawnQuietMoveTable.GetMoves(sourceIx, color);
                quiets &= ~position.GetOccupied();

                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                captures &= position.GetOccupied(color.Other());

                foreach (var piece in new[] {PieceType.Queen, PieceType.Knight, PieceType.Rook, PieceType.Bishop})
                {
                    var quietsTmp = quiets;
                    //foreach (var dstIx in Bits.Enumerate(quiets))
                    while (Bits.TryPopLsb(ref quietsTmp, out var dstIx))
                    {
                        moves.Add(new Move(MoveType.Promotion|MoveType.Quiet, sourceIx, dstIx, piece));
                    }

                    var capturesTmp = captures;
                    //foreach (var dstIx in Bits.Enumerate(captures))
                    while (Bits.TryPopLsb(ref capturesTmp, out var dstIx))
                    {
                        moves.Add(new Move(MoveType.Promotion|MoveType.Capture, sourceIx, dstIx, piece));
                    }
                }
            }
        }

        private void GenerateKnightMoves(List<Move> moves, Position position, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = KnightMoveTable.GetMoves(sourceIx);
                dstSquares &= ~position.GetOccupied(position.SideToMove);

                GenerateMoves(moves, MoveType.Normal, PieceType.Knight, sourceIx, dstSquares, position.GetOccupied());
            }
        }

        private void GenerateBishopMoves(List<Move> moves, Position position, PieceType pieceType, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = BishopMoveTable.GetMoves(sourceIx, position.GetOccupied());
                dstSquares &= ~position.GetOccupied(position.SideToMove);

                GenerateMoves(moves, MoveType.Normal, pieceType, sourceIx, dstSquares, position.GetOccupied());
            }
        }

        private void GenerateRookMoves(List<Move> moves, Position position, PieceType pieceType, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = RookMoveTable.GetMoves(sourceIx, position.GetOccupied());
                dstSquares &= ~position.GetOccupied(position.SideToMove);

                GenerateMoves(moves, MoveType.Normal, pieceType, sourceIx, dstSquares, position.GetOccupied());
            }
        }

        private void GenerateKingNormalMoves(List<Move> moves, Position position, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = KingMoveTable.GetMoves(sourceIx);
                // don't allow king to move on piece of same color
                dstSquares &= ~position.GetOccupied(position.SideToMove);

                GenerateMoves(moves, MoveType.Normal, PieceType.King, sourceIx, dstSquares, position.GetOccupied());
            }
        }

        private void GenerateKingCastling(List<Move> moves, Position position)
        {
            var castlingDsts = position.CastlingRights & CastlingTables.GetCastlingRightsDstColorMask(position.SideToMove);
            //foreach (var dstIx in Bits.Enumerate(castlingDsts))
            while (Bits.TryPopLsb(ref castlingDsts, out var dstIx))
            {
                if ((position.GetOccupied() & CastlingTables.GetCastlingEmptySquares(dstIx)) > 0)
                    continue;

                // TODO: do these incheck validations as part of move validation? not sure...
                if (position.InCheck(position.SideToMove))
                    continue;

                bool attacked = false;
                foreach (var potentiallyAttackedIx in Bits.Enumerate(CastlingTables.GetCastlingAttacks(dstIx)))
                {
                    if (position.IsAttacked(potentiallyAttackedIx, position.SideToMove))
                    {
                        attacked = true;
                        break;
                    }
                }

                if (attacked)
                    continue;

                // if we still have castling rights, the inbetween squares aren't occupied, we aren't in check, and we aren't castling through or into check, then we should be good to go!
                int kingIx = Bits.GetLsb(position.GetPieceBitboard(position.SideToMove, PieceType.King));
                moves.Add(new Move(MoveType.Castling|MoveType.Quiet, kingIx, dstIx));
            }
        }
        
        private static (ulong quiets, ulong captures) SeparateQuietsCaptures(ulong moves, ulong occupancy)
        {
            ulong quiets = moves & ~occupancy;
            ulong captures = moves & ~quiets;
            return (quiets, captures);
        }

        private void GenerateMoves(List<Move> moves, MoveType moveType, PieceType pieceType, int sourceIx, ulong dstSquares, ulong occupancy)
        {
            var (quiets, captures) = SeparateQuietsCaptures(dstSquares, occupancy);

            //foreach (var dstIx in Bits.Enumerate(quiets))
            while (Bits.TryPopLsb(ref quiets, out var dstIx))
            {
                moves.Add(new Move(moveType|MoveType.Quiet, sourceIx, dstIx));
            }

            //foreach (var dstIx in Bits.Enumerate(captures))
            while (Bits.TryPopLsb(ref captures, out var dstIx))
            {
                moves.Add(new Move(moveType|MoveType.Capture, sourceIx, dstIx));
            }
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
                    promotionSources |= Position.ValueFromFileRank(file, promotionRank);
                }

                ret[(int)color] = promotionSources;
            }

            return ret;
        }

        
    }
}
