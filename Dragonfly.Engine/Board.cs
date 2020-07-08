﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Dragonfly.Engine.MoveGens;
using Dragonfly.Engine.PerformanceTypes;

namespace Dragonfly.Engine
{
    public sealed class Board
    {
        private BitboardArray _pieceBitboards;
        private PieceSquareArray _squares;
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
        public int RepetitionNumber { get; private set; }

        // Takes ownership of all arrays passed to it; they should not be changed after the board is created.
        public Board(Piece[] pieceSquares, Color sideToMove, ulong castlingRights, ulong enPassant, int fiftyMoveCounter, int fullMove)
        {
            // safer to go by the length the PieceSquareArray, because the Piece[] will always error on index out of bounds
            for (int i = 0; i < _squares.Length; i++)
                _squares[i] = pieceSquares[i];

            PopulatePieceBitboardsFromSquares(ref _pieceBitboards, pieceSquares);

            _occupiedWhite = CalculateOccupied(Color.White);
            _occupiedBlack = CalculateOccupied(Color.Black);

            _occupied = _occupiedWhite | _occupiedBlack;

            _parent = null;

            SideToMove = sideToMove;
            CastlingRights = castlingRights;
            EnPassant = enPassant;

            FiftyMoveCounter = fiftyMoveCounter;
            GamePly = GamePlyFromFullMove(fullMove, sideToMove);
            _historyPly = 0;

            RepetitionNumber = 1;

            ZobristHash = ZobristHashing.CalculateFullHash(this);
        }

        private static void PopulatePieceBitboardsFromSquares(ref BitboardArray pieceBitboards, Piece[] squares)
        {
            for (int i = 0; i < pieceBitboards.Length; i++)
                pieceBitboards[i] = 0;

            for (int ix = 0; ix < 64; ix++)
            {
                var piece = squares[ix];
                if (piece.PieceType == PieceType.None)
                    continue;
                
                var value = ValueFromIx(ix);
                pieceBitboards[PieceBitboardIndex(piece.Color, piece.PieceType)] |= value;
            }
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
            _squares = parent.CopySquares();

            var pieceType = MovePiece(move.SourceIx, move.DstIx);
            
            _parent = parent;

            SetBoardData(move, 0, pieceType == PieceType.Pawn);
        }

        private Board(Board parent, Move move, NormalCaptureMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();
            _squares = parent.CopySquares();

            RemovePiece(move.DstIx);
            MovePiece(move.SourceIx, move.DstIx);

            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, DoublePawnMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();
            _squares = parent.CopySquares();

            MovePiece(move.SourceIx, move.DstIx, parent.SideToMove, PieceType.Pawn);

            _parent = parent;

            SetBoardData(move, ValueFromIx((move.SourceIx + move.DstIx) / 2), true);
        }

        private Board(Board parent, Move move, EnPassantMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();
            _squares = parent.CopySquares();

            var (srcFile, srcRank) = FileRankFromIx(move.SourceIx);
            var (dstFile, dstRank) = FileRankFromIx(move.DstIx);
            var capturedPawnIx = IxFromFileRank(dstFile, srcRank);

            RemovePiece(capturedPawnIx, parent.SideToMove.Other(), PieceType.Pawn);
            MovePiece(move.SourceIx, move.DstIx, parent.SideToMove, PieceType.Pawn);
            
            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, PromotionQuietMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();
            _squares = parent.CopySquares();

            RemovePiece(move.SourceIx, parent.SideToMove, PieceType.Pawn);
            AddPiece(move.DstIx, parent.SideToMove, move.PromotionPiece);
            
            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, PromotionCaptureMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();
            _squares = parent.CopySquares();

            RemovePiece(move.SourceIx, parent.SideToMove, PieceType.Pawn);
            RemovePiece(move.DstIx);
            AddPiece(move.DstIx, parent.SideToMove, move.PromotionPiece);
            
            _parent = parent;

            SetBoardData(move, 0, true);
        }

        private Board(Board parent, Move move, CastlingMove x)
        {
            _pieceBitboards = parent.CopyPieceBitboards();
            _squares = parent.CopySquares();

            var rookSrcIx = CastlingTables.GetCastlingRookSrcIx(move.DstIx);
            var rookDstIx = CastlingTables.GetCastlingRookDstIx(move.DstIx);

            MovePiece(move.SourceIx, move.DstIx, parent.SideToMove, PieceType.King);
            MovePiece(rookSrcIx, rookDstIx, parent.SideToMove, PieceType.Rook);

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
            FiftyMoveCounter = CalculateFiftyMoveCounter(_parent, captureOrPawnMove);
            GamePly = _parent.GamePly + 1;
            _historyPly = _parent._historyPly + 1;

            ZobristHash = _parent.ZobristHash ^ ZobristHashing.CalculateHashDiff(_parent, this);
            RepetitionNumber = CalculateRepetitionNumber(this);
        }

        private void AddPiece(int ix, Color color, PieceType pieceType)
        {
            Debug.Assert(_squares[ix].PieceType == PieceType.None);
            _pieceBitboards[PieceBitboardIndex(color, pieceType)] |= ValueFromIx(ix);
            _squares[ix] = new Piece(color, pieceType);
            
            // TODO: zobrist hash update
            // TODO: material hash update
            // TODO: pawn hash update
        }

        private void RemovePiece(int ix, Color color, PieceType pieceType)
        {
            Debug.Assert(_squares[ix] == new Piece(color, pieceType));
            _pieceBitboards[PieceBitboardIndex(color, pieceType)] &= ~ValueFromIx(ix);
            _squares[ix] = Piece.None;

            // TODO: zobrist hash update
            // TODO: material hash update
            // TODO: pawn hash update
        }

        private void MovePiece(int srcIx, int dstIx, Color color, PieceType pieceType)
        {
            Debug.Assert(_squares[srcIx] == new Piece(color, pieceType));
            Debug.Assert(_squares[dstIx].PieceType == PieceType.None);
            _pieceBitboards[PieceBitboardIndex(color, pieceType)] &= ~ValueFromIx(srcIx);
            _pieceBitboards[PieceBitboardIndex(color, pieceType)] |= ValueFromIx(dstIx);
            _squares[srcIx] = Piece.None;
            _squares[dstIx] = new Piece(color, pieceType);

            // TODO: zobrist hash update
            // No material hash update needed
            // TODO: pawn hash update
        }

        private PieceType RemovePiece(int ix)
        {
            var piece = _squares[ix];
            Debug.Assert(piece.PieceType != PieceType.None);
            RemovePiece(ix, piece.Color, piece.PieceType);
            return piece.PieceType;
        }

        private PieceType MovePiece(int srcIx, int dstIx)
        {
            var piece = _squares[srcIx];
            Debug.Assert(piece.PieceType != PieceType.None);
            MovePiece(srcIx, dstIx, piece.Color, piece.PieceType);
            return piece.PieceType;
        }

        private static int CalculateFiftyMoveCounter(Board parent, bool captureOrPawnMove)
        {
            int fiftyMoveCounter;
            if (captureOrPawnMove)
                fiftyMoveCounter = 0;
            else
                fiftyMoveCounter = parent.FiftyMoveCounter + 1;

            return fiftyMoveCounter;
        }

        // TODO: default to 2 for repetition count when the board we're calculating for is within search
        private static int CalculateRepetitionNumber(Board board)
        {
            var zobristHash = board.ZobristHash;

            // calculate rep count
            for (int i = board.FiftyMoveCounter - 2; i >= 0; i -= 2)
            {
                var tmpBoard = board._parent?._parent;
                if (tmpBoard == null)
                    return 1;

                board = tmpBoard;

                if (board.ZobristHash == zobristHash)
                    return board.RepetitionNumber + 1;
            }

            return 1;
        }

        public void AssertBitboardsMatchSquares()
        {
            for (int ix = 0; ix < 64; ix++)
            {
                var piece = _squares[ix];
                if (piece.PieceType == PieceType.None)
                {
                    if ((GetOccupied() & ValueFromIx(ix)) > 0)
                        throw new Exception("Bitboards do not match squares");
                }
                else
                {
                    if ((GetPieceBitboard(piece.Color, piece.PieceType) & ValueFromIx(ix)) == 0)
                        throw new Exception("Bitboards do not match squares");
                }
            }
        }

        #endregion FromMoves

        public override string ToString()
        {
            return BoardParsing.FenStringFromBoard(this);
        }

        // would need to copy the contents if this were an array
        private BitboardArray CopyPieceBitboards()
        {
            return _pieceBitboards;
        }

        // would need to copy the contents if this were an array
        private PieceSquareArray CopySquares()
        {
            return _squares;
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

        public Piece GetPiece(int ix)
        {
            return _squares[ix];
        }

        public PieceType GetPieceType(int ix)
        {
            return GetPiece(ix).PieceType;
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

        public static int PieceBitboardIndex(Piece piece)
        {
            return PieceBitboardIndex(piece.Color, piece.PieceType);
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
