using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpHeart.Engine.Interfaces;

namespace SharpHeart.Engine.MoveGens
{
    public sealed class MoveGen : IMoveGen
    {
        private static readonly ulong[] PawnPromotionSourceTable = GeneratePawnPromotionSourceTable();
        
        public void Generate(List<Move> moves, Board board)
        {
            GeneratePawnMoves(moves, board, board.GetPieceBitboard(board.SideToMove, PieceType.Pawn));
            GenerateKnightMoves(moves, board, board.GetPieceBitboard(board.SideToMove, PieceType.Knight));
            // we can combine generation of queen with bishop and rook moves because when we store moves in the move list, we don't record the piece type which is moving
            GenerateBishopMoves(
                moves,
                board,
                PieceType.Bishop,
                board.GetPieceBitboard(board.SideToMove, PieceType.Bishop)
            );
            GenerateBishopMoves(
                moves,
                board,
                PieceType.Queen,
                board.GetPieceBitboard(board.SideToMove, PieceType.Queen)
            );
            GenerateRookMoves(
                moves, 
                board, 
                PieceType.Rook,
                board.GetPieceBitboard(board.SideToMove, PieceType.Rook)
            );
            GenerateRookMoves(
                moves,
                board,
                PieceType.Queen,
                board.GetPieceBitboard(board.SideToMove, PieceType.Queen)
            );
            GenerateKingNormalMoves(
                moves,
                board,
                board.GetPieceBitboard(board.SideToMove, PieceType.King)
            );
            GenerateKingCastling(
                moves,
                board
            );
        }

        public Move GetMoveFromCoordinateString(Board b, string coordinateString)
        {
            if (TryGetMoveFromCoordinateString(b, coordinateString, out Move move))
                return move;
            else
                throw new Exception($"Could not find move: \"{coordinateString}\"");
        }

        public bool TryGetMoveFromCoordinateString(Board b, string coordinateString, out Move move)
        {
            List<Move> moves = new List<Move>();
            Generate(moves, b);

            foreach (var tmpMove in moves)
            {
                var potentialCoordinateString = BoardParsing.MoveToCoordinateString(tmpMove);
                if (string.Equals(potentialCoordinateString, coordinateString, StringComparison.OrdinalIgnoreCase))
                {
                    move = tmpMove;
                    return true;
                }
            }

            move = default;
            return false;
        }

        private void GeneratePawnMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            var color = board.SideToMove;
            ulong promotionSourceSquares = sourceSquares & PawnPromotionSourceTable[(int) color];
            ulong nonPromotionSourceSquares = sourceSquares & ~promotionSourceSquares;

            //foreach (var sourceIx in Bits.Enumerate(nonPromotionSourceSquares))
            while (Bits.TryPopLsb(ref nonPromotionSourceSquares, out var sourceIx))
            {
                // TODO: separate out normal moves from double moves. Normal moves use normal lookup table, double moves use magic lookup table
                var quiets = PawnQuietMoveTable.GetMoves(sourceIx, color);
                quiets &= ~board.GetOccupied();

                var doubles = PawnDoubleMoveTable.GetMoves(sourceIx, board.GetOccupied(), color);
                // note: no need to mask double moves; the move table already takes care of that

                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                var normalCaptures = captures & board.GetOccupied(color.Other());
                var enPassantCaptures = captures & board.EnPassant;

                //foreach (var dstIx in Bits.Enumerate(quiets))
                while (Bits.TryPopLsb(ref quiets, out var dstIx))
                {
                    moves.Add(Move.Make(MoveType.Normal, MoveType.Quiet, PieceType.Pawn, sourceIx, dstIx));
                }

                //foreach (var dstIx in Bits.Enumerate(doubles))
                while (Bits.TryPopLsb(ref doubles, out var dstIx))
                {
                    moves.Add(Move.Make(MoveType.DoublePawnMove, MoveType.Quiet, PieceType.Pawn, sourceIx, dstIx));
                }

                //foreach (var dstIx in Bits.Enumerate(normalCaptures))
                while (Bits.TryPopLsb(ref normalCaptures, out var dstIx))
                {
                    moves.Add(Move.Make(MoveType.Normal, MoveType.Capture, PieceType.Pawn, sourceIx, dstIx));
                }

                //foreach (var dstIx in Bits.Enumerate(enPassantCaptures))
                while (Bits.TryPopLsb(ref enPassantCaptures, out var dstIx))
                {
                    moves.Add(Move.Make(MoveType.EnPassant, MoveType.Capture, PieceType.Pawn, sourceIx, dstIx));
                }
            }

            //foreach (var sourceIx in Bits.Enumerate(promotionSourceSquares))
            while (Bits.TryPopLsb(ref promotionSourceSquares, out var sourceIx))
            {
                var quiets = PawnQuietMoveTable.GetMoves(sourceIx, color);
                quiets &= ~board.GetOccupied();

                var captures = PawnCaptureMoveTable.GetMoves(sourceIx, color);
                captures &= board.GetOccupied(color.Other());

                foreach (var piece in new[] {PieceType.Queen, PieceType.Knight, PieceType.Rook, PieceType.Bishop})
                {
                    //foreach (var dstIx in Bits.Enumerate(quiets))
                    while (Bits.TryPopLsb(ref quiets, out var dstIx))
                    {
                        moves.Add(Move.Make(MoveType.Promotion, MoveType.Quiet, PieceType.Pawn, sourceIx, dstIx, piece));
                    }

                    //foreach (var dstIx in Bits.Enumerate(captures))
                    while (Bits.TryPopLsb(ref captures, out var dstIx))
                    {
                        moves.Add(Move.Make(MoveType.Promotion, MoveType.Capture, PieceType.Pawn, sourceIx, dstIx, piece));
                    }
                }
            }
        }

        private void GenerateKnightMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = KnightMoveTable.GetMoves(sourceIx);
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                GenerateMoves(moves, MoveType.Normal, PieceType.Knight, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateBishopMoves(List<Move> moves, Board board, PieceType pieceType, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = BishopMoveTable.GetMoves(sourceIx, board.GetOccupied());
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                GenerateMoves(moves, MoveType.Normal, pieceType, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateRookMoves(List<Move> moves, Board board, PieceType pieceType, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = RookMoveTable.GetMoves(sourceIx, board.GetOccupied());
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                GenerateMoves(moves, MoveType.Normal, pieceType, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateKingNormalMoves(List<Move> moves, Board board, ulong sourceSquares)
        {
            //foreach (var sourceIx in Bits.Enumerate(sourceSquares))
            while (Bits.TryPopLsb(ref sourceSquares, out var sourceIx))
            {
                var dstSquares = KingMoveTable.GetMoves(sourceIx);
                // don't allow king to move on piece of same color
                dstSquares &= ~board.GetOccupied(board.SideToMove);

                GenerateMoves(moves, MoveType.Normal, PieceType.King, sourceIx, dstSquares, board.GetOccupied());
            }
        }

        private void GenerateKingCastling(List<Move> moves, Board board)
        {
            var castlingDsts = board.CastlingRights & CastlingTables.GetCastlingRightsDstColorMask(board.SideToMove);
            //foreach (var dstIx in Bits.Enumerate(castlingDsts))
            while (Bits.TryPopLsb(ref castlingDsts, out var dstIx))
            {
                if ((board.GetOccupied() & CastlingTables.GetCastlingEmptySquares(dstIx)) > 0)
                    continue;

                // TODO: do these incheck validations as part of move validation? not sure...
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
                int kingIx = Bits.GetLsb(board.GetPieceBitboard(board.SideToMove, PieceType.King));
                moves.Add(Move.Make(MoveType.Castling, MoveType.Quiet, PieceType.King, kingIx, dstIx));
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
                moves.Add(Move.Make(moveType, MoveType.Quiet, pieceType, sourceIx, dstIx));
            }

            //foreach (var dstIx in Bits.Enumerate(captures))
            while (Bits.TryPopLsb(ref captures, out var dstIx))
            {
                moves.Add(Move.Make(moveType, MoveType.Capture, pieceType, sourceIx, dstIx));
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
                    promotionSources |= Board.ValueFromFileRank(file, promotionRank);
                }

                ret[(int)color] = promotionSources;
            }

            return ret;
        }

        
    }
}
