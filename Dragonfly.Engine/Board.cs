using System;
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
        private Board? _parent;

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

        public Board()
        {}

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

        #region MakeMove and its helpers

        public static Board MakeMove(Board shell, Move move, Board parent)
        {
            // copy data which doesn't depend on performing the move
            shell._pieceBitboards = parent.CopyPieceBitboards();
            shell._squares = parent.CopySquares();
            shell.ZobristHash = parent.ZobristHash;
            shell._parent = parent;
            shell._inCheckWhite = null;
            shell._inCheckBlack = null;
            shell.SideToMove = parent.SideToMove.Other();
            shell.EnPassant = 0; // this gets set for double moves
            shell.FiftyMoveCounter = CalculateFiftyMoveCounter(parent, move);
            shell.GamePly = parent.GamePly + 1;
            shell._historyPly = parent._historyPly + 1;

            switch (move.MoveType)
            {
                case MoveType.Normal | MoveType.Quiet:
                    shell.MakeNormalQuietMove(move);
                    break;
                case MoveType.Normal | MoveType.Capture:
                    shell.MakeNormalCaptureMove(move);
                    break;
                case MoveType.DoubleMove | MoveType.Quiet:
                    shell.MakeDoublePawnMove(move);
                    break;
                case MoveType.EnPassant | MoveType.Capture:
                    shell.MakeEnPassantMove(move);
                    break;
                case MoveType.Promotion | MoveType.Quiet:
                    shell.MakePromotionQuietMove(move);
                    break;
                case MoveType.Promotion | MoveType.Capture:
                    shell.MakePromotionCaptureMove(move);
                    break;
                case MoveType.Castling | MoveType.Quiet:
                    shell.MakeCastlingMove(move);
                    break;
                default:
                    throw new Exception($"Invalid move type: {move.MoveType}");
            }

            shell.CastlingRights = parent.CastlingRights & CastlingTables.GetCastlingUpdateMask(move);
            shell._occupiedWhite = shell.CalculateOccupied(Color.White);
            shell._occupiedBlack = shell.CalculateOccupied(Color.Black);
            shell._occupied = shell._occupiedWhite | shell._occupiedBlack;
            shell.ZobristHash ^= ZobristHashing.OtherHashDiff(parent, shell);
            shell.RepetitionNumber = CalculateRepetitionNumber(shell);

            return shell;
        }

        private Color SideMoving => SideToMove.Other();

        private void MakeNormalQuietMove(Move move)
        {
            MovePiece(move.SourceIx, move.DstIx);
        }

        private void MakeNormalCaptureMove(Move move)
        {
            RemovePiece(move.DstIx);
            MovePiece(move.SourceIx, move.DstIx);
        }

        private void MakeDoublePawnMove(Move move)
        {
            MovePiece(move.SourceIx, move.DstIx, SideMoving, PieceType.Pawn);

            EnPassant = ValueFromIx((move.SourceIx + move.DstIx) / 2);
        }

        private void MakeEnPassantMove(Move move)
        {
            var (srcFile, srcRank) = FileRankFromIx(move.SourceIx);
            var (dstFile, dstRank) = FileRankFromIx(move.DstIx);
            var capturedPawnIx = IxFromFileRank(dstFile, srcRank);

            RemovePiece(capturedPawnIx, SideMoving.Other(), PieceType.Pawn);
            MovePiece(move.SourceIx, move.DstIx, SideMoving, PieceType.Pawn);
        }

        private void MakePromotionQuietMove(Move move)
        {
            RemovePiece(move.SourceIx, SideMoving, PieceType.Pawn);
            AddPiece(move.DstIx, SideMoving, move.PromotionPiece);
        }

        private void MakePromotionCaptureMove(Move move)
        {
            RemovePiece(move.SourceIx, SideMoving, PieceType.Pawn);
            RemovePiece(move.DstIx);
            AddPiece(move.DstIx, SideMoving, move.PromotionPiece);
        }

        private void MakeCastlingMove(Move move)
        {
            var rookSrcIx = CastlingTables.GetCastlingRookSrcIx(move.DstIx);
            var rookDstIx = CastlingTables.GetCastlingRookDstIx(move.DstIx);

            MovePiece(move.SourceIx, move.DstIx, SideMoving, PieceType.King);
            MovePiece(rookSrcIx, rookDstIx, SideMoving, PieceType.Rook);
        }

        private void AddPiece(int ix, Color color, PieceType pieceType)
        {
            Debug.Assert(_squares[ix].PieceType == PieceType.None);
            _pieceBitboards[PieceBitboardIndex(color, pieceType)] |= ValueFromIx(ix);
            _squares[ix] = new Piece(color, pieceType);
            
            ZobristHash ^= ZobristHashing.CalculatePieceBitboardHashDiff(ValueFromIx(ix), color, pieceType);
            // TODO: material hash update
            // TODO: pawn hash update
        }

        private void RemovePiece(int ix, Color color, PieceType pieceType)
        {
            Debug.Assert(_squares[ix] == new Piece(color, pieceType));
            _pieceBitboards[PieceBitboardIndex(color, pieceType)] &= ~ValueFromIx(ix);
            _squares[ix] = Piece.None;

            ZobristHash ^= ZobristHashing.CalculatePieceBitboardHashDiff(ValueFromIx(ix), color, pieceType);
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

            ZobristHash ^= ZobristHashing.CalculatePieceBitboardHashDiff(
                ValueFromIx(srcIx) | ValueFromIx(dstIx),
                color,
                pieceType);
            // No material hash update needed
            // TODO: pawn hash update
        }

        private void RemovePiece(int ix)
        {
            var piece = _squares[ix];
            Debug.Assert(piece.PieceType != PieceType.None);
            RemovePiece(ix, piece.Color, piece.PieceType);
        }

        private void MovePiece(int srcIx, int dstIx)
        {
            var piece = _squares[srcIx];
            Debug.Assert(piece.PieceType != PieceType.None);
            MovePiece(srcIx, dstIx, piece.Color, piece.PieceType);
        }

        private static int CalculateFiftyMoveCounter(Board parent, Move move)
        {
            if (
                move.MoveType.HasFlag(MoveType.Capture) ||
                move.MoveType.HasFlag(MoveType.DoubleMove) ||
                move.MoveType.HasFlag(MoveType.Promotion) ||
                parent._squares[move.SourceIx].PieceType == PieceType.Pawn
            )
            {
                return 0;
            }
            else
            {
                return parent.FiftyMoveCounter + 1;
            }
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
