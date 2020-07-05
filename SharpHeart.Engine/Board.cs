using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.Engine
{
    public sealed class Board
    {
        private readonly ulong[] _pieceBitboards;
        private readonly ulong _occupiedWhite;
        private readonly ulong _occupiedBlack;
        private readonly ulong _occupied;
        private bool? _inCheckWhite;
        private bool? _inCheckBlack;
        private readonly Board? _parent;

        public Color SideToMove { get; private set; }
        public ulong EnPassant { get; private set; }
        public ulong CastlingRights { get; private set; }
        public int HalfmoveCounter { get; private set; }
        public int FullMove { get; private set; }

        // Takes ownership of all arrays passed to it; they should not be changed after the board is created.
        private Board(ulong[] pieceBitboards, Color sideToMove, ulong castlingRights, ulong enPassant)
        {
            _pieceBitboards = pieceBitboards;
            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);

            _occupied = CalculateOccupied();

            SideToMove = sideToMove;
            CastlingRights = castlingRights;
            EnPassant = enPassant;
        }

        // Takes ownership of all arrays passed to it; they should not be changed after the board is created.
        public Board(ulong[] pieceBitboards, Color sideToMove, ulong castlingRights, ulong enPassant, Board parent, bool captureOrPawnMove)
            : this(pieceBitboards, sideToMove, castlingRights, enPassant)
        {
            if (captureOrPawnMove)
                HalfmoveCounter = 0;
            else
                HalfmoveCounter = parent.HalfmoveCounter + 1;

            if (sideToMove == Color.White)
                FullMove = parent.FullMove + 1;
            else
                FullMove = parent.FullMove;

            _parent = parent;
        }

        // Takes ownership of all arrays passed to it; they should not be changed after the board is created.
        public Board(ulong[] pieceBitboards, Color sideToMove, ulong castlingRights, ulong enPassant, int halfmoveCounter, int fullMove)
            :this(pieceBitboards, sideToMove, castlingRights, enPassant)
        {
            HalfmoveCounter = halfmoveCounter;
            FullMove = fullMove;
            
            _parent = null;
        }

        #region MyRegion

        // used to select appropriate constructors

        private struct NormalQuietMove{}
        private struct NormalCaptureMove{}
        private struct DoublePawnMove{}
        private struct EnPassantMove{}
        private struct PromotionQuietMove{}
        private struct PromotionCaptureMove{}
        private struct CastlingQuietMove{}

        public Board DoMove(Move move)
        {
            var parent = this;
            switch (move.MoveType)
            {
                case MoveType.Pawn | MoveType.Normal | MoveType.Quiet:
                case MoveType.Normal | MoveType.Quiet:
                    return new Board(parent, move, new NormalQuietMove());
                case MoveType.Normal | MoveType.Capture:
                case MoveType.Pawn | MoveType.Normal | MoveType.Capture:
                    return new Board(parent, move, new NormalCaptureMove());
                case MoveType.Pawn | MoveType.DoubleMove | MoveType.Quiet:
                    return new Board(parent, move, new DoublePawnMove());
                case MoveType.Pawn | MoveType.EnPassant | MoveType.Capture:
                    return new Board(parent, move, new EnPassantMove());
                case MoveType.Pawn | MoveType.Promotion | MoveType.Quiet:
                    return new Board(parent, move, new PromotionQuietMove());
                case MoveType.Pawn | MoveType.Promotion | MoveType.Capture:
                    return new Board(parent, move, new PromotionCaptureMove());
                case MoveType.Castling | MoveType.Quiet:
                    return new Board(parent, move, new CastlingQuietMove());
                default:
                    throw new Exception($"Invalid move type: {move.MoveType}");
            }
        }

        private Board(Board parent, Move move, NormalQuietMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & Board.ValueFromIx(move.DstIx)) == 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= Board.ValueFromIx(move.DstIx);

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = 0;
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, move.PieceType == PieceType.Pawn);
            FullMove = CalculateFullMove(parent);
        }

        private Board(Board parent, Move move, NormalCaptureMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied(parent.SideToMove.Other()) & Board.ValueFromIx(move.DstIx)) > 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= Board.ValueFromIx(move.DstIx);

            for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
            {
                _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove.Other(), pieceType)] &= ~Board.ValueFromIx(move.DstIx);
            }

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = 0;
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, true);
            FullMove = CalculateFullMove(parent);
        }

        private Board(Board parent, Move move, DoublePawnMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & Board.ValueFromIx(move.DstIx)) == 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= Board.ValueFromIx(move.DstIx);

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = Board.ValueFromIx((move.SourceIx + move.DstIx) / 2);
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, true);
            FullMove = CalculateFullMove(parent);
        }

        private Board(Board parent, Move move, EnPassantMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            var (srcFile, srcRank) = Board.FileRankFromIx(move.SourceIx);
            var (dstFile, dstRank) = Board.FileRankFromIx(move.DstIx);
            var capturedPawnIx = Board.IxFromFileRank(dstFile, srcRank);

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & Board.ValueFromIx(move.DstIx)) == 0);
            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove.Other(), PieceType.Pawn)] & Board.ValueFromIx(capturedPawnIx)) > 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= Board.ValueFromIx(move.DstIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove.Other(), PieceType.Pawn)] &= ~Board.ValueFromIx(capturedPawnIx);

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = 0;
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, true);
            FullMove = CalculateFullMove(parent);
        }

        private Board(Board parent, Move move, PromotionQuietMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & Board.ValueFromIx(move.DstIx)) == 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PromotionPiece)] |= Board.ValueFromIx(move.DstIx);

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = 0;
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, true);
            FullMove = CalculateFullMove(parent);
        }

        private Board(Board parent, Move move, PromotionCaptureMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied(parent.SideToMove.Other()) & Board.ValueFromIx(move.DstIx)) > 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PromotionPiece)] |= Board.ValueFromIx(move.DstIx);

            for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
            {
                _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove.Other(), pieceType)] &= ~Board.ValueFromIx(move.DstIx);
            }

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = 0;
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, true);
            FullMove = CalculateFullMove(parent);
        }

        private Board(Board parent, Move move, CastlingQuietMove x)
        {
            _pieceBitboards = parent.GetPieceBitboards();

            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] & Board.ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & Board.ValueFromIx(move.DstIx)) == 0);
            Debug.Assert((parent.GetOccupied() & CastlingTables.GetCastlingEmptySquares(move.DstIx)) == 0);
            Debug.Assert((_pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, PieceType.Rook)] & CastlingTables.GetCastlingRookSrcValue(move.DstIx)) > 0);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~Board.ValueFromIx(move.SourceIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= Board.ValueFromIx(move.DstIx);

            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, PieceType.Rook)] &= ~CastlingTables.GetCastlingRookSrcValue(move.DstIx);
            _pieceBitboards[Board.PieceBitboardIndex(parent.SideToMove, PieceType.Rook)] |= CastlingTables.GetCastlingRookDstValue(move.DstIx);

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;
            _parent = parent;

            SideToMove = parent.SideToMove.Other();
            EnPassant = 0;
            CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            HalfmoveCounter = CalculateHalfmoveCounter(parent, false);
            FullMove = CalculateFullMove(parent);
        }

        private static int CalculateHalfmoveCounter(Board parent, bool captureOrPawnMove)
        {
            int halfmoveCounter;
            if (captureOrPawnMove)
                halfmoveCounter = 0;
            else
                halfmoveCounter = parent.HalfmoveCounter + 1;

            return halfmoveCounter;
        }

        private static int CalculateFullMove(Board parent)
        {
            int fullMove;
            if (parent.SideToMove == Color.White)
                fullMove = parent.FullMove + 1;
            else
                fullMove = parent.FullMove;

            return fullMove;
        }

        #endregion FromMoves


        public ulong[] GetPieceBitboards()
        {
            var ret = new ulong[12];
            for (int i = 0; i < 12; i++)
                ret[i] = _pieceBitboards[i];
            
            return ret;
        }

        public ulong GetPieceBitboard(Color c, PieceType pt)
        {
            return _pieceBitboards[PieceBitboardIndex(c, pt)];
        }

        private ulong CalculateOccupied(Color c)
        {
            return 
                GetPieceBitboard(c, PieceType.Pawn) |
                GetPieceBitboard(c, PieceType.Bishop) |
                GetPieceBitboard(c, PieceType.Knight) |
                GetPieceBitboard(c, PieceType.Rook) |
                GetPieceBitboard(c, PieceType.Queen) |
                GetPieceBitboard(c, PieceType.King);
        }

        public ulong GetOccupied(Color c)
        {
            if (c == Color.White)
                return _occupiedWhite;
            else
                return _occupiedBlack;
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
            if (side == Color.White)
                return InCheckWhite();
            else
                return InCheckBlack();
        }

        private bool InCheckWhite()
        {
            if (!_inCheckWhite.HasValue)
            {
                _inCheckWhite = IsAttacked(Bits.GetLsb(GetPieceBitboard(Color.White, PieceType.King)), Color.White);
            }

            return _inCheckWhite.Value;
        }

        private bool InCheckBlack()
        {
            if (!_inCheckBlack.HasValue)
            {
                _inCheckBlack = IsAttacked(Bits.GetLsb(GetPieceBitboard(Color.Black, PieceType.King)), Color.Black);
            }

            return _inCheckBlack.Value;
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
                    if ((value & GetPieceBitboard(color, pieceType)) > 0)
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
            if ((potentialRookCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.Rook)) > 0)
                return true;

            if ((potentialRookCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.Queen)) > 0)
                return true;

            // bishop and queen
            var potentialBishopCaptures = BishopMoveTable.GetMoves(defenderIx, GetOccupied());
            if ((potentialBishopCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.Bishop)) > 0)
                return true;

            if ((potentialBishopCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.Queen)) > 0)
                return true;

            var potentialKnightCaptures = KnightMoveTable.GetMoves(defenderIx);
            if ((potentialKnightCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.Knight)) > 0)
                return true;

            var potentialKingCaptures = KingMoveTable.GetMoves(defenderIx);
            if ((potentialKingCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.King)) > 0)
                return true;

            var potentialPawnCaptures = PawnCaptureMoveTable.GetMoves(defenderIx, defenderColor);
            if ((potentialPawnCaptures & GetPieceBitboard(defenderColor.Other(), PieceType.Pawn)) > 0)
                return true;

            // we checked all possible attacks by all possible piece types
            return false;
        }

        #region Static methods

        public static int PieceBitboardIndex(Color color, PieceType pieceType)
        {
            return ((int) color * 6) + (int) pieceType;
        }

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
