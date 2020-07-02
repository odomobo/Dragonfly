using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.Engine
{
    public sealed class Board
    {
        private readonly ulong[][] _pieceBitboards;
        private readonly ulong[] _occupiedColor;
        private readonly ulong _occupied;
        private readonly bool?[] _inCheck;
        private readonly Board? _parent;

        public Color SideToMove { get; private set; }
        public ulong EnPassant { get; private set; }
        public ulong CastlingRights { get; private set; }

        // Takes ownership of all arrays passed to it; they should not be changed after the board is created.
        public Board(ulong[][] pieceBitboards, Color sideToMove, ulong castlingRights, ulong enPassant, Board parent = null)
        {
            // TODO: clone the arrays so we don't get issues if the caller modifies them??? Seems expensive
            _pieceBitboards = pieceBitboards;
            _occupiedColor = new ulong[2];
            _inCheck = new bool?[2];
            
            for (int i = 0; i < _occupiedColor.Length; i++)
                _occupiedColor[i] = CalculateOccupied((Color)i);

            _occupied = CalculateOccupied();

            SideToMove = sideToMove;
            EnPassant = enPassant;
            CastlingRights = castlingRights;

            _parent = parent;

            // TODO: set other values
        }

        public ulong[][] GetPieceBitboards()
        {
            return new[]
            {
                (ulong[])_pieceBitboards[0].Clone(), // TODO: do this differently?
                (ulong[])_pieceBitboards[1].Clone(),
            };
        }

        public ulong GetPieceBitboard(PieceType pt, Color c)
        {
            return _pieceBitboards[(int)c][(int)pt];
        }

        private ulong CalculateOccupied(Color c)
        {
            return 
                GetPieceBitboard(PieceType.Pawn, c) |
                GetPieceBitboard(PieceType.Bishop, c) |
                GetPieceBitboard(PieceType.Knight, c) |
                GetPieceBitboard(PieceType.Rook, c) |
                GetPieceBitboard(PieceType.Queen, c) |
                GetPieceBitboard(PieceType.King, c);
        }

        public ulong GetOccupied(Color c)
        {
            return _occupiedColor[(int) c];
        }

        private ulong CalculateOccupied()
        {
            return GetOccupied(Color.Black) | GetOccupied(Color.White);
        }

        public ulong GetOccupied()
        {
            return _occupied;
        }

        public bool InCheck(Color side)
        {
            var inCheck = _inCheck[(int) side];
            if (!inCheck.HasValue)
            {
                inCheck = IsAttacked(Bits.GetLsb(GetPieceBitboard(PieceType.King, side)), side);
                _inCheck[(int) side] = inCheck;
            }

            return inCheck.Value;
        }

        public (PieceType pieceType, Color color) GetPieceTypeColor(int ix)
        {
            var value = ValueFromIx(ix);
            foreach (var color in new[] { Color.White, Color.Black })
            {
                if ((value & GetOccupied(color)) == 0)
                    continue;

                // if we make it here, it's definitely one of these pieces
                for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType = pieceType + 1)
                {
                    if ((value & GetPieceBitboard(pieceType, color)) > 0)
                        return (pieceType, color);
                }
                throw new Exception();
            }

            return (PieceType.None, Color.White);
        }

        public PieceType GetPieceType(int ix)
        {
            var (pieceType, _) = GetPieceTypeColor(ix);
            return pieceType;
        }

        // Note that this doesn't check if a pawn can be taken en passant
        public bool IsAttacked(int defenderIx, Color defenderColor)
        {
            // check in roughly descending order of power

            // rook and queen
            var potentialRookCaptures = RookMoveTable.GetMoves(defenderIx, GetOccupied());
            if ((potentialRookCaptures & GetPieceBitboard(PieceType.Rook, defenderColor.Other())) > 0)
                return true;

            if ((potentialRookCaptures & GetPieceBitboard(PieceType.Queen, defenderColor.Other())) > 0)
                return true;

            // bishop and queen
            var potentialBishopCaptures = BishopMoveTable.GetMoves(defenderIx, GetOccupied());
            if ((potentialBishopCaptures & GetPieceBitboard(PieceType.Bishop, defenderColor.Other())) > 0)
                return true;

            if ((potentialBishopCaptures & GetPieceBitboard(PieceType.Queen, defenderColor.Other())) > 0)
                return true;

            var potentialKnightCaptures = KnightMoveTable.GetMoves(defenderIx);
            if ((potentialKnightCaptures & GetPieceBitboard(PieceType.Knight, defenderColor.Other())) > 0)
                return true;

            var potentialKingCaptures = KingMoveTable.GetMoves(defenderIx);
            if ((potentialKingCaptures & GetPieceBitboard(PieceType.King, defenderColor.Other())) > 0)
                return true;

            var potentialPawnCaptures = PawnCaptureMoveTable.GetMoves(defenderIx, defenderColor);
            if ((potentialPawnCaptures & GetPieceBitboard(PieceType.Pawn, defenderColor.Other())) > 0)
                return true;

            // we checked all possible attacks by all possible piece types
            return false;
        }

        #region Static methods

        public static int IxFromFileRank(int file, int rank)
        {
            return rank*8 + file;
        }

        public static (int file, int rank) FileRankFromIx(int ix)
        {
            var rank = ix / 8;
            var file = ix % 8;
            return (file, rank);
        }

        public static bool FileRankOnBoard(int file, int rank)
        {
            return file >= 0 && file < 8 && rank >= 0 && rank < 8;
        }

        public static ulong ValueFromIx(int ix)
        {
            return 1UL << ix;
        }

        public static ulong ValueFromFileRank(int file, int rank)
        {
            return ValueFromIx(IxFromFileRank(file, rank));
        }

        public static IEnumerable<(int file, int rank)> GetAllFilesRanks()
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    yield return (file, rank);
                }
            }
        }

        #endregion
    }
}
