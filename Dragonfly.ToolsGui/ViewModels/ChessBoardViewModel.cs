using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.MoveGeneration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.ViewModels
{
    public class ChessBoardViewModel : ViewModelBase
    {
        public ObservableCollection<ChessBoardSquareViewModel> SquaresCollection { get; set; }
        public readonly ChessBoardSquareViewModel[,] Squares;

        public Position Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
                PositionUpdated();
            }
        }
        private Position _position;

        public ChessBoardViewModel()
        {
            SquaresCollection = new ObservableCollection<ChessBoardSquareViewModel>();
            Squares = new ChessBoardSquareViewModel[8,8];

            for (int rowIx = 0; rowIx < 8; rowIx++)
            {
                for (int columnIx = 0; columnIx < 8; columnIx++)
                {
                    var square = new ChessBoardSquareViewModel(rowIx, columnIx, this);
                    SquaresCollection.Add(square);
                    Squares[square.File, square.Rank] = square;
                }
            }

            Position = BoardParsing.PositionFromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); // initial fen
        }

        private void PositionUpdated()
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Squares[file, rank].Piece = Position.GetPieceFromFileRank(file, rank);
                }
            }
        }

        public void TryMakeMove(int sourceSquare, int destinationSquare)
        {
            if (!BoardParsing.TryGetMoveFromSourceDestinationPromoteQueen(new MoveGenerator(), Position, sourceSquare, destinationSquare, out var move))
                return;

            Position = Position.MakeMove(move);
        }
    }
}
