using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.MoveGeneration.Tables;

namespace Dragonfly.Engine
{
    // TODO: rename to something better?
    public static class BoardParsing
    {
        public static Position PositionFromFen(string fen)
        {
            Piece[] squares = new Piece[64];
            for (int i = 0; i < squares.Length; i++)
                squares[i] = new Piece(Color.White, PieceType.None);

            var splitFen = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (splitFen.Count < 1)
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            if (splitFen.Count < 2)
                splitFen.Add("w");

            if (splitFen.Count < 3)
                splitFen.Add("-");

            if (splitFen.Count < 4)
                splitFen.Add("-");

            if (splitFen.Count < 5)
                splitFen.Add("0");

            if (splitFen.Count < 6)
                splitFen.Add("1");

            var strBoard = splitFen[0];
            var strToMove = splitFen[1];
            var strCastling = splitFen[2];
            var strEnPassant = splitFen[3];
            var strHalfmoveCounter = splitFen[4];
            var strFullMoves = splitFen[5];

            var splitBoardState = strBoard.Split('/');
            if (splitBoardState.Length != 8)
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            for (int i = 0; i < 8; i++)
            {
                int rank = 7 - i; // go from 7 to 0 instead of 0 to 7
                int file = 0;

                foreach (var c in splitBoardState[i])
                {
                    if (c >= '1' && c <= '8')
                    {
                        file += c - '0';
                        continue;
                    }

                    if (TryParsePiece(c, out var pieceType, out var color))
                    {
                        var ix = Position.IxFromFileRank(file, rank);
                        squares[ix] = new Piece(color, pieceType);

                        file++;
                        continue;
                    }

                    // couldn't parse the character
                    throw new Exception("Bad FEN string"); // TODO: better error type & message
                }
                if (file != 8)
                    throw new Exception("Bad FEN string"); // TODO: better error type & message
            }

            Color sideToMove;
            if (strToMove == "w")
                sideToMove = Color.White;
            else if (strToMove == "b")
                sideToMove = Color.Black;
            else
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            ulong castlingRights = 0;
            var castlingHash = new HashSet<char>(strCastling);
            if (castlingHash.Contains('K'))
                castlingRights |= CastlingTables.WhiteKingsideDst;
            if (castlingHash.Contains('Q'))
                castlingRights |= CastlingTables.WhiteQueensideDst;
            if (castlingHash.Contains('k'))
                castlingRights |= CastlingTables.BlackKingsideDst;
            if (castlingHash.Contains('q'))
                castlingRights |= CastlingTables.BlackQueensideDst;

            ulong enPassant;
            if (strEnPassant == "-")
            {
                enPassant = 0;
            }
            else
            {
                var enPassantIx = IxFromSquareStr(strEnPassant);
                enPassant= Position.ValueFromIx(enPassantIx);
            }

            // TODO: move counts
            int halfmoveCounter = int.Parse(strHalfmoveCounter);
            int fullMove = int.Parse(strFullMoves);
            
            return new Position(squares, sideToMove, castlingRights, enPassant, halfmoveCounter, fullMove);
        }

        public static string FenStringFromBoard(Position position)
        {
            var fenBuilder = new StringBuilder();
            for (int rank = 7; rank >= 0; rank--)
            {
                int skipped = 0;
                for (int file = 0; file < 8; file++)
                {
                    var (color, pieceType) = position.GetPiece(Position.IxFromFileRank(file, rank));
                    if (pieceType == PieceType.None)
                    {
                        skipped++;
                        continue;
                    }

                    // if we found a piece, first let's fill in the count of any skipped moves
                    if (skipped > 0)
                    {
                        fenBuilder.Append(skipped);
                        skipped = 0;
                    }

                    fenBuilder.Append(LetterFromPieceTypeColor(pieceType, color));
                }

                // count any final skipped moves
                if (skipped > 0)
                    fenBuilder.Append(skipped);

                if (rank > 0)
                    fenBuilder.Append("/");
            }

            var colorStr = position.SideToMove == Color.White ? "w" : "b";

            return $"{fenBuilder} {colorStr} {CastlingStrFromValue(position.CastlingRights)} {SquareStrFromValue(position.EnPassant)} {position.FiftyMoveCounter} {position.FullMove}";
        }

        public static bool TryParsePiece(char pieceChar, out PieceType pieceType, out Color color)
        {
            if (pieceChar >= 'a' && pieceChar <= 'z')
            {
                color = Color.Black;
            }
            else
            {
                // lowercase
                pieceChar = (char)(pieceChar - 'A' + 'a');
                color = Color.White;
            }

            switch (pieceChar)
            {
                case 'p':
                    pieceType = PieceType.Pawn;
                    break;
                case 'n':
                    pieceType = PieceType.Knight;
                    break;
                case 'b':
                    pieceType = PieceType.Bishop;
                    break;
                case 'r':
                    pieceType = PieceType.Rook;
                    break;
                case 'q':
                    pieceType = PieceType.Queen;
                    break;
                case 'k':
                    pieceType = PieceType.King;
                    break;
                default:
                    color = Color.White;
                    pieceType = PieceType.None;
                    return false;
            }

            return true;
        }

        public static Move GetMoveFromCoordinateString(IMoveGenerator moveGenerator, Position b, string coordinateString)
        {
            if (TryGetMoveFromCoordinateString(moveGenerator, b, coordinateString, out Move move))
                return move;
            else
                throw new Exception($"Could not find move: \"{coordinateString}\"");
        }

        public static bool TryGetMoveFromCoordinateString(
            IMoveGenerator moveGenerator,
            Position b,
            string coordinateString,
            out Move move)
        {
            List<Move> moves = new List<Move>();
            moveGenerator.Generate(moves, b);

            foreach (var tmpMove in moves)
            {
                var potentialCoordinateString = CoordinateStringFromMove(tmpMove);
                if (!string.Equals(potentialCoordinateString, coordinateString, StringComparison.OrdinalIgnoreCase))
                    continue;

                // if a pseudolegal move generator, then we need to make sure that the move we're attempting is even legal
                if (!moveGenerator.OnlyLegalMoves)
                {
                    var testingBoard = Position.MakeMove(new Position(), tmpMove, b);

                    // if we moved into check, clearly it was an invalid move
                    if (testingBoard.MovedIntoCheck())
                        continue;
                }

                move = tmpMove;
                return true;
            }

            move = default; // in practice, this is fine, because callers should never use this if returning false
            return false;
        }

        public static char LetterFromPieceType(PieceType pieceType)
        {
            // will return uppercase, which is what we want
            return LetterFromPieceTypeColor(pieceType, Color.White);
        }

        public static char LetterFromPieceTypeColor(PieceType pieceType, Color color)
        {
            int charCase = 0;
            if (color == Color.Black)
                charCase = 'a' - 'A';
            
            switch (pieceType)
            {
                case PieceType.Pawn:
                    return (char)('P' + charCase);
                case PieceType.Knight:
                    return (char)('N' + charCase);
                case PieceType.Bishop:
                    return (char)('B' + charCase);
                case PieceType.Rook:
                    return (char)('R' + charCase);
                case PieceType.Queen:
                    return (char)('Q' + charCase);
                case PieceType.King:
                    return (char)('K' + charCase);
                default:
                    return '?';
            }
        }

        public static char UnicodePieceFromPieceTypeColor(PieceType pieceType, Color color)
        {
            if (color == Color.White)
            {
                switch (pieceType)
                {
                    case PieceType.Pawn:
                        return '♙';
                    case PieceType.Knight:
                        return '♘';
                    case PieceType.Bishop:
                        return '♗';
                    case PieceType.Rook:
                        return '♖';
                    case PieceType.Queen:
                        return '♕';
                    case PieceType.King:
                        return '♔';
                    default:
                        return '?';
                }
            }
            else
            {
                switch (pieceType)
                {
                    case PieceType.Pawn:
                        return '♟';
                    case PieceType.Knight:
                        return '♞';
                    case PieceType.Bishop:
                        return '♝';
                    case PieceType.Rook:
                        return '♜';
                    case PieceType.Queen:
                        return '♛';
                    case PieceType.King:
                        return '♚';
                    default:
                        return '?';
                }
            }
        }

        public static string SanPrefixFromPieceType(PieceType pieceType)
        {
            if (pieceType == PieceType.Pawn)
                return string.Empty;
            else
                return LetterFromPieceType(pieceType).ToString();
        }

        public static string NaiveSanStringFromMoveBoard(Move move, Position position)
        {
            var pieceType = position.GetPieceType(move.SourceIx);
            string pieceTypeStr = SanPrefixFromPieceType(pieceType);

            string captureStr = "";
            if ((move.MoveType & MoveType.Capture) > 0)
            {
                if (pieceType == PieceType.Pawn)
                    captureStr = FileStrFromIx(move.SourceIx);

                captureStr += "x";
            }

            string dstSquareStr = SquareStrFromIx(move.DstIx);

            string promotionPieceStr = "";
            if ((move.MoveType & MoveType.Promotion) > 0)
            {
                promotionPieceStr = LetterFromPieceType(move.PromotionPiece).ToString();
            }

            return $"{pieceTypeStr}{captureStr}{dstSquareStr}{promotionPieceStr}";
        }

        public static string NaiveSanStringFromMove(Move move)
        {
            string sourceSquareStr = SquareStrFromIx(move.SourceIx);

            string captureStr = "";
            if ((move.MoveType & MoveType.Capture) > 0)
                captureStr = "x";

            string dstSquareStr = SquareStrFromIx(move.DstIx);

            string promotionPieceStr = "";
            if ((move.MoveType & MoveType.Promotion) > 0)
            {
                promotionPieceStr = LetterFromPieceType(move.PromotionPiece).ToString();
            }

            return $"{sourceSquareStr}{captureStr}{dstSquareStr}{promotionPieceStr}";
        }

        public static string CoordinateStringFromMove(Move m)
        {
            // Move type 0 means null move
            if (m.MoveType == 0)
                return "0000";

            string promotionPieceStr = "";
            if ((m.MoveType & MoveType.Promotion) > 0)
                promotionPieceStr = LetterFromPieceTypeColor(m.PromotionPiece, Color.Black).ToString(); // lowercase

            return $"{SquareStrFromIx(m.SourceIx)}{SquareStrFromIx(m.DstIx)}{promotionPieceStr}";
        }

        public static string FileStrFromIx(int ix)
        {
            var (file, rank) = Position.FileRankFromIx(ix);
            return $"{(char)('a' + file)}";
        }

        public static string SquareStrFromValue(ulong value)
        {
            if (value == 0)
                return "-";

            return SquareStrFromIx(Bits.GetLsb(value));
        }

        public static string SquareStrFromIx(int ix)
        {
            var (file, rank) = Position.FileRankFromIx(ix);
            return $"{(char)('a' + file)}{rank + 1}";
        }

        public static bool TryGetIxFromSquareStr(string square, out int ix)
        {
            ix = 0;

            if (square.Length != 2)
                return false;

            square = square.ToLower();

            int file = square[0] - 'a';
            int rank = square[1] - '1';

            if (file < 0 || file > 7 || rank < 0 || rank > 7)
                return false;

            ix = Position.IxFromFileRank(file, rank);
            return true;
        }

        public static int IxFromSquareStr(string square)
        {
            if (!TryGetIxFromSquareStr(square, out int ret))
                throw new Exception($"Invalid square string: \"{square}\"");

            return ret;
        }

        public static string CastlingStrFromValue(ulong castlingRights)
        {
            string ret = "";
            if ((castlingRights & CastlingTables.WhiteKingsideDst) > 0)
                ret += "K";
            if ((castlingRights & CastlingTables.WhiteQueensideDst) > 0)
                ret += "Q";
            if ((castlingRights & CastlingTables.BlackKingsideDst) > 0)
                ret += "k";
            if ((castlingRights & CastlingTables.BlackQueensideDst) > 0)
                ret += "q";

            if (ret == "")
                ret = "-";

            return ret;
        }
    }
}
