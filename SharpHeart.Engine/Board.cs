using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using SharpHeart.Engine.MoveGen;

namespace SharpHeart.Engine
{
    public sealed class Board
    {
        private readonly ulong[][] _pieceBitboards;
        private readonly ulong[] _occupiedColor;
        private readonly ulong _occupied;
        private readonly bool?[] _inCheck;
        public Color SideToMove { get; private set; }
        public ulong EnPassant { get; private set; }
        public ulong CastlingRights { get; private set; }

        // won't be used until we start reusing board objects
        public Board()
        {
            _pieceBitboards = new ulong[(int)PieceType.Count][];
            _occupiedColor = new ulong[2];
            _inCheck = new bool?[2];
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                _pieceBitboards[i] = new ulong[2];
            }

            for (int i = 0; i < _occupiedColor.Length; i++)
                _occupiedColor[i] = CalculateOccupied((Color) i);

            _occupied = CalculateOccupied();

            // TODO: set other values?? maybe

            throw new NotImplementedException();
        }

        public Board(ulong[] whitePieceBitboards, ulong[] blackPieceBitboards, Color sideToMove, ulong castlingRights)
        {
            // TODO: clone the arrays so we don't get issues if the caller modifies them
            _pieceBitboards = new ulong[(int)PieceType.Count][];
            _occupiedColor = new ulong[2];
            _inCheck = new bool?[2];
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                _pieceBitboards[i] = new ulong[2];
                _pieceBitboards[i][(int) Color.Black] = blackPieceBitboards[i];
                _pieceBitboards[i][(int) Color.White] = whitePieceBitboards[i];
            }

            for (int i = 0; i < _occupiedColor.Length; i++)
                _occupiedColor[i] = CalculateOccupied((Color)i);

            _occupied = CalculateOccupied();

            SideToMove = sideToMove;
            EnPassant = 0;
            CastlingRights = castlingRights;

            // TODO: set other values
        }

        public Board(ulong[] whitePieceBitboards, ulong[] blackPieceBitboards, Color sideToMove, ulong castlingRights, Board parent)
        {
            // TODO: clone the arrays so we don't get issues if the caller modifies them
            _pieceBitboards = new ulong[(int)PieceType.Count][];
            _occupiedColor = new ulong[2];
            _inCheck = new bool?[2];
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                _pieceBitboards[i] = new ulong[2];
                _pieceBitboards[i][(int)Color.Black] = blackPieceBitboards[i];
                _pieceBitboards[i][(int)Color.White] = whitePieceBitboards[i];
            }

            for (int i = 0; i < _occupiedColor.Length; i++)
                _occupiedColor[i] = CalculateOccupied((Color)i);

            _occupied = CalculateOccupied();

            SideToMove = sideToMove;
            EnPassant = 0;
            CastlingRights = castlingRights;

            // TODO: set other values
        }

        // Won't use this until we start reusing board objects
        public void Clear()
        {
            for (int i = 0; i < _pieceBitboards.Length; i++)
            {
                Array.Clear(_pieceBitboards[i], 0, _pieceBitboards[i].Length);
            }

            // TODO
            throw new NotImplementedException();
        }

        public ulong GetPieceBitboard(PieceType pt, Color c)
        {
            return _pieceBitboards[(int) pt][(int) c];
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

        // TODO history and stuff?

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
