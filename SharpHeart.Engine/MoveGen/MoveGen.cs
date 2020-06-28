﻿using System;
using System.Collections.Generic;
using System.Text;
using SharpHeart.Engine.Interfaces;

namespace SharpHeart.Engine.MoveGen
{
    public sealed class MoveGen : IMoveGen
    {
        private static readonly ulong[] PawnPromotionSourceTable = GeneratePawnPromotionSourceTable();
        
        public void Generate(List<Move> moves, Board board)
        {
            GeneratePawnMoves(moves, board, board.GetPieceBitboard(PieceType.Pawn, board.SideToMove));
            GenerateKnightMoves(moves, board, board.GetPieceBitboard(PieceType.Knight, board.SideToMove));
            // we can combine generation of queen with bishop and rook moves because when we store moves in the move list, we don't record the piece type which is moving
            GenerateBishopMoves(
                moves,
                board,
                board.GetPieceBitboard(PieceType.Bishop, board.SideToMove) |
                board.GetPieceBitboard(PieceType.Queen, board.SideToMove)
            );
            GenerateRookMoves(
                moves, 
                board, 
                board.GetPieceBitboard(PieceType.Rook, board.SideToMove) | 
                board.GetPieceBitboard(PieceType.Queen, board.SideToMove)
            );
            GenerateKingNormalMoves(
                moves,
                board,
                board.GetPieceBitboard(PieceType.King, board.SideToMove)
            );
            GenerateKingCastling(
                moves,
                board
            );
        }

        private void GeneratePawnMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            var color = board.SideToMove;
            ulong promotionSourceSquares = sourceSquares & PawnPromotionSourceTable[(int) color];
            ulong nonPromotionSourceSquares = sourceSquares & ~promotionSourceSquares;

            foreach (var sourceIx in Bits.Enumerate(nonPromotionSourceSquares))
            {
                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                var normalCaptures = captures & board.GetOccupied(color.Other());
                var enPassantCaptures = captures & board.EnPassant;

                // TODO: separate out normal moves from double moves. Normal moves use normal lookup table, double moves use magic lookup table
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

        private void GenerateKnightMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = KnightMoveTable.GetMoves(sourceIx);
                // don't allow knight to move on piece of same color
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                GenerateMoves(moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateBishopMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = BishopMoveTable.GetMoves(sourceIx, board.GetOccupied());
                GenerateMoves(moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateRookMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = RookMoveTable.GetMoves(sourceIx, board.GetOccupied());
                GenerateMoves(moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateKingNormalMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            {
                var dstSquares = KingMoveTable.GetMoves(sourceIx);
                // don't allow king to move on piece of same color
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                foreach (var dstIx in Bits.Enumerate(dstSquares))
                {
                    GenerateMoves(moves, MoveType.Normal, sourceIx, dstSquares, board.GetOccupied());
                }
            }
        }

        private void GenerateKingCastling(List<Move> moves, Board board)
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
                    if (board.IsAttacked(potentiallyAttackedIx, board.SideToMove))
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
        
        private static (ulong quiets, ulong captures) SeparateQuietsCaptures(ulong moves, ulong occupancy)
        {
            ulong quiets = moves & ~occupancy;
            ulong captures = moves & ~quiets;
            return (quiets, captures);
        }

        private void GenerateMoves(List<Move> moves, MoveType moveType, int sourceIx, ulong dstSquares, ulong occupancy)
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
