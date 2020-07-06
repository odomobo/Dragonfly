using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Dragonfly.Engine.MoveGens;

namespace Dragonfly.Engine
{
    public sealed class Board
    {
        // TODO: could replace this with a struct that acts like a ulong[12]
        private readonly ulong[] _pieceBitboards;
        private ulong _occupiedWhite;
        private ulong _occupiedBlack;
        private ulong _occupied;
        private bool? _inCheckWhite;
        private bool? _inCheckBlack;
        private readonly Board? _parent;

        public Color SideToMove { get; private set; }
        public ulong EnPassant { get; private set; }
        public ulong CastlingRights { get; private set; }
        public int FiftyMoveCounter { get; private set; }
        public int GamePly { get; private set; } // game's fullmove number, starting from 0
        public int FullMove => GamePly/2 + 1;
        private int _historyPly; // similar to game ply, but from the position we started from, not from the initial position in the game

        public ulong ZobristHash { get; private set; }

        // Takes ownership of all arrays passed to it; they should not be changed after the board is created.
        public Board(ulong[] pieceBitboards, Color sideToMove, ulong castlingRights, ulong enPassant, int fiftyMoveCounter, int fullMove)
        {
            _pieceBitboards = pieceBitboards;
            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);

            _occupied = _occupiedWhite | _occupiedBlack;

            SideToMove = sideToMove;
            CastlingRights = castlingRights;
            EnPassant = enPassant;

            FiftyMoveCounter = fiftyMoveCounter;
            GamePly = GamePlyFromFullMove(fullMove, sideToMove);
            _historyPly = 0;

            ZobristHash = ZobristHashing.CalculateFullHash(this);

            _parent = null;
        }

        private static int GamePlyFromFullMove(int fullMove, Color sideToMove)
        {
            if (sideToMove == Color.White)
                return (fullMove - 1) * 2;
            else
                return (fullMove - 1) * 2 + 1;
        }

        #region DoMove and its constructors

        // used to select appropriate constructors
        private struct NormalQuietMove{}
        private struct NormalCaptureMove{}
        private struct DoublePawnMove{}
        private struct EnPassantMove{}
        private struct PromotionQuietMove{}
        private struct PromotionCaptureMove{}
        private struct CastlingMove{}

        public Board DoMove(Move move)
        {
            var parent = this;
            switch (move.MoveType)
            {
                case MoveType.Normal | MoveType.Quiet:
                    return new Board(parent, move, new NormalQuietMove());
                case MoveType.Normal | MoveType.Capture:
                    return new Board(parent, move, new NormalCaptureMove());
                case MoveType.DoubleMove | MoveType.Quiet:
                    return new Board(parent, move, new DoublePawnMove());
                case MoveType.EnPassant | MoveType.Capture:
                    return new Board(parent, move, new EnPassantMove());
                case MoveType.Promotion | MoveType.Quiet:
                    return new Board(parent, move, new PromotionQuietMove());
                case MoveType.Promotion | MoveType.Capture:
                    return new Board(parent, move, new PromotionCaptureMove());
                case MoveType.Castling | MoveType.Quiet:
                    return new Board(parent, move, new CastlingMove());
                default:
                    throw new Exception($"Invalid move type: {move.MoveType}");
            }
        }

        private Board(Board parent, Move move, NormalQuietMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & ValueFromIx(move.DstIx)) == 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= ValueFromIx(move.DstIx);

            _parent = parent;

            SetBoardData(move, 0, move.PieceType == PieceType.Pawn);
        }

        private Board(Board parent, Move move, NormalCaptureMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied(parent.SideToMove.Other()) & ValueFromIx(move.DstIx)) > 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= ValueFromIx(move.DstIx);

            for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
            {
                _pieceBitboards[PieceBitboardIndex(parent.SideToMove.Other(), pieceType)] &= ~ValueFromIx(move.DstIx);
            }

            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, DoublePawnMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & ValueFromIx(move.DstIx)) == 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= ValueFromIx(move.DstIx);

            _parent = parent;

            SetBoardData(move, ValueFromIx((move.SourceIx + move.DstIx) / 2), true);
        }

        private Board(Board parent, Move move, EnPassantMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            var (srcFile, srcRank) = FileRankFromIx(move.SourceIx);
            var (dstFile, dstRank) = FileRankFromIx(move.DstIx);
            var capturedPawnIx = IxFromFileRank(dstFile, srcRank);

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & ValueFromIx(move.DstIx)) == 0);
            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove.Other(), PieceType.Pawn)] & ValueFromIx(capturedPawnIx)) > 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= ValueFromIx(move.DstIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove.Other(), PieceType.Pawn)] &= ~ValueFromIx(capturedPawnIx);

            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, PromotionQuietMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & ValueFromIx(move.DstIx)) == 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PromotionPiece)] |= ValueFromIx(move.DstIx);

            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, PromotionCaptureMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied(parent.SideToMove.Other()) & ValueFromIx(move.DstIx)) > 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PromotionPiece)] |= ValueFromIx(move.DstIx);

            for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
            {
                _pieceBitboards[PieceBitboardIndex(parent.SideToMove.Other(), pieceType)] &= ~ValueFromIx(move.DstIx);
            }

            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, CastlingMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();

            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] & ValueFromIx(move.SourceIx)) > 0);
            Debug.Assert((parent.GetOccupied() & ValueFromIx(move.DstIx)) == 0);
            Debug.Assert((parent.GetOccupied() & CastlingTables.GetCastlingEmptySquares(move.DstIx)) == 0);
            Debug.Assert((_pieceBitboards[PieceBitboardIndex(parent.SideToMove, PieceType.Rook)] & CastlingTables.GetCastlingRookSrcValue(move.DstIx)) > 0);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] &= ~ValueFromIx(move.SourceIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, move.PieceType)] |= ValueFromIx(move.DstIx);

            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, PieceType.Rook)] &= ~CastlingTables.GetCastlingRookSrcValue(move.DstIx);
            _pieceBitboards[PieceBitboardIndex(parent.SideToMove, PieceType.Rook)] |= CastlingTables.GetCastlingRookDstValue(move.DstIx);

            _parent = parent;

            SetBoardData(move, 0, false);
        }

        /// <summary>
        /// This should only be called from DoMove board constructors
        /// </summary>
        /// <param name="move"></param>
        /// <param name="enPassant"></param>
        /// <param name="captureOrPawnMove"></param>
        private void SetBoardData(Move move, ulong enPassant, bool captureOrPawnMove)
        {
            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);
            _occupied = _occupiedWhite | _occupiedBlack;

            // this can only be called if parent isn't null
            SideToMove = _parent!.SideToMove.Other();
            EnPassant = enPassant;
            CastlingRights = _parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            FiftyMoveCounter = CalculateHalfmoveCounter(_parent, captureOrPawnMove);
            GamePly = _parent.GamePly + 1;
            _historyPly = _parent._historyPly + 1;

            ZobristHash = _parent.ZobristHash ^ ZobristHashing.CalculateHashDiff(_parent, this);
        }

        private static int CalculateHalfmoveCounter(Board parent, bool captureOrPawnMove)
        {
            int halfmoveCounter;
            if (captureOrPawnMove)
                halfmoveCounter = 0;
            else
                halfmoveCounter = parent.FiftyMoveCounter + 1;

            return halfmoveCounter;
        }

        #endregion FromMoves

        public override string ToString()
        {
            return BoardParsing.FenStringFromBoard(this);
        }

        private ulong[] CopyPieceBitboards()
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
            ulong ret = 0;
            // this needs to match calculation used for GetPieceBitboard
            for (int i = (int)c * 6; i < (int)c * 6 + (int)PieceType.Count; i++)
                ret |= _pieceBitboards[i];

            return ret;
        }

        public ulong GetOccupied(Color c)
        {
            if (c == Color.White)
                return _occupiedWhite;
            else
                return _occupiedBlack;
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
