using Avalonia.Input;
using Avalonia.Media.Imaging;
using Dragonfly.Engine.CoreTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.DebugFrontend.ViewModels
{
    public class ChessBoardSquareViewModel : ViewModelBase
    {
        public int RowIx { get; }
        public int ColumnIx { get; }
        public int File { get; }
        public int Rank { get; }
        public string SquareName { get; }
        public bool IsLight { get; }
        public string SquareColor => IsLight ? "#dee3e6" : "#8ca2ad";
        public string PieceString
        {
            get
            {
                if (Piece == Piece.None)
                    return "";

                if (Piece.Color == Color.White)
                {
                    switch (Piece.PieceType)
                    {
                        case PieceType.Pawn:
                            return "♙";
                        case PieceType.Knight:
                            return "♘";
                        case PieceType.Bishop:
                            return "♗";
                        case PieceType.Rook:
                            return "♖";
                        case PieceType.Queen:
                            return "♕";
                        case PieceType.King:
                            return "♔";
                    }
                }
                else
                {
                    switch (Piece.PieceType)
                    {
                        case PieceType.Pawn:
                            return "♟";
                        case PieceType.Knight:
                            return "♞";
                        case PieceType.Bishop:
                            return "♝";
                        case PieceType.Rook:
                            return "♜";
                        case PieceType.Queen:
                            return "♛";
                        case PieceType.King:
                            return "♚";
                    }
                }
                return "?";
            }
        }

        public ChessBoardViewModel? Parent { get; set; }

        public Bitmap PieceImage
        {
            get
            {
                if (Piece == Piece.None)
                    return Images.Instance.None;

                if (Piece.Color == Color.White)
                {
                    switch (Piece.PieceType)
                    {
                        case PieceType.Pawn:
                            return Images.Instance.WP;
                        case PieceType.Knight:
                            return Images.Instance.WN;
                        case PieceType.Bishop:
                            return Images.Instance.WB;
                        case PieceType.Rook:
                            return Images.Instance.WR;
                        case PieceType.Queen:
                            return Images.Instance.WQ;
                        case PieceType.King:
                            return Images.Instance.WK;
                    }
                }
                else
                {
                    switch (Piece.PieceType)
                    {
                        case PieceType.Pawn:
                            return Images.Instance.BP;
                        case PieceType.Knight:
                            return Images.Instance.BN;
                        case PieceType.Bishop:
                            return Images.Instance.BB;
                        case PieceType.Rook:
                            return Images.Instance.BR;
                        case PieceType.Queen:
                            return Images.Instance.BQ;
                        case PieceType.King:
                            return Images.Instance.BK;
                    }
                }
                return Images.Instance.Error;
            }
        }

        public Piece Piece
        {
            get => _piece;
            set
            {
                _piece = value;
                OnPropertyChanged(nameof(Piece));
                OnPropertyChanged(nameof(PieceString));
                OnPropertyChanged(nameof(PieceImage));
            }
        }
        private Piece _piece;

        public ChessBoardSquareViewModel(int rowIx, int columnIx, ChessBoardViewModel parent)
        {
            RowIx = rowIx;
            ColumnIx = columnIx;
            Parent = parent;

            File = ColumnIx;
            Rank = 7 - RowIx;
            SquareName = $"{(char)('a'+File)}{1+Rank}";
            IsLight = (Rank + File) % 2 != 0;
            Piece = Piece.None;
        }

        public ChessBoardSquareViewModel()
            :this(0, 0, null)
        {
        }

        public void Drop(object? sender, DragEventArgs e)
        {
            if (Parent == null)
                return;

            if (!e.Data.Contains("ChessMoveSourceSquare"))
                return;

            var sourceSquare = e.Data.Get("ChessMoveSourceSquare") as int?;
            if (sourceSquare == null)
                return;

            var destinationSquare = Position.IxFromFileRank(File, Rank);

            Parent.TryMakeMove(sourceSquare.Value, destinationSquare);
        }
    }
}
