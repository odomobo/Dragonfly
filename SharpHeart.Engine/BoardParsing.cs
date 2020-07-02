using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpHeart.Engine
{
    // TODO: rename to something nicer
    public static class BoardParsing
    {
        public static Board FromFen(string fen)
        {
            // TODO: single pieceBitboards instead of this
            ulong[] pieceBitboard = new ulong[12];

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

            var splitBoardState = splitFen[0].Split('/');
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

                    if (BoardParsing.TryParsePiece(c, out var pieceType, out var color))
                    {
                        var value = Board.ValueFromFileRank(file, rank);
                        pieceBitboard[Board.PieceBitboardIndex(color, pieceType)] |= value;

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
            if (splitFen[1] == "w")
                sideToMove = Color.White;
            else if (splitFen[1] == "b")
                sideToMove = Color.Black;
            else
                throw new Exception("Bad FEN string"); // TODO: better error type & message

            // TODO: castling rights
            ulong castlingRights = 0;
            // TODO: en passant
            ulong enPassant = 0;
            // TODO: move counts

            return new Board(pieceBitboard, sideToMove, castlingRights, enPassant);
        }

        public static bool TryParsePiece(char originalC, out PieceType pieceType, out Color color)
        {
            var c = originalC;
            if (c >= 'a' && c <= 'z')
            {
                color = Color.Black;
            }
            else
            {
                // lowercase
                c = (char)(c - 'A' + 'a');
                color = Color.White;
            }

            switch (c)
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

        public static char PieceTypeToLetter(PieceType pieceType)
        {
            // will return uppercase, which is what we want
            return PieceTypeColorToLetter(pieceType, Color.White);
        }

        public static char PieceTypeColorToLetter(PieceType pieceType, Color color)
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

        public static char PieceTypeColorToUnicodePiece(PieceType pieceType, Color color)
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

        public static string PieceTypeToSanPrefix(PieceType pieceType)
        {
            if (pieceType == PieceType.Pawn)
                return string.Empty;
            else
                return PieceTypeToLetter(pieceType).ToString();
        }

        public static string MoveToNaiveSanString(Move m)
        {
            string pieceTypeStr = PieceTypeToSanPrefix(m.PieceType);

            string captureStr = "";
            if ((m.MoveType & MoveType.Capture) > 0)
            {
                if (m.PieceType == PieceType.Pawn)
                    captureStr = FileStrFromIx(m.SourceIx);

                captureStr += "x";
            }

            string dstSquareStr = SquareStrFromIx(m.DstIx);

            string promotionPieceStr = "";
            if ((m.MoveType & MoveType.Promotion) > 0)
            {
                promotionPieceStr = PieceTypeToLetter(m.PromotionPiece).ToString();
            }

            return $"{pieceTypeStr}{captureStr}{dstSquareStr}{promotionPieceStr}";
        }

        public static string MoveToCoordinateString(Move m)
        {
            string promotionPieceStr = "";
            if ((m.MoveType & MoveType.Promotion) > 0)
                promotionPieceStr = PieceTypeColorToLetter(m.PromotionPiece, Color.Black).ToString(); // lowercase

            return $"{SquareStrFromIx(m.SourceIx)}{SquareStrFromIx(m.DstIx)}{promotionPieceStr}";
        }

        public static string FileStrFromIx(int ix)
        {
            var (file, rank) = Board.FileRankFromIx(ix);
            return $"{(char)('a' + file)}";
        }

        public static string SquareStrFromIx(int ix)
        {
            var (file, rank) = Board.FileRankFromIx(ix);
            return $"{(char)('a' + file)}{rank + 1}";
        }
    }
}
