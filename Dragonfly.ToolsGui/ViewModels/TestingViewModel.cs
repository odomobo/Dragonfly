using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Dragonfly.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.ToolsGui.ViewModels
{
    public class TestingViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        public int ClickedCount { get => _clickedCount; set { _clickedCount = value; OnPropertyChanged(nameof(ClickedCount)); } }
        private int _clickedCount = 0;
        public ChessBoardViewModel ChessBoardViewModel { get; set; }

        public string Log { get => _log; set { _log = value; OnPropertyChanged(nameof(Log)); } }
        private string _log = "";

        public TestingViewModel()
        {
            _clickedCount = 10;
            ChessBoardViewModel = new ChessBoardViewModel();
            ClickHello = new RelayCommand(Click);
        }

        public RelayCommand ClickHello { get; }

        //public RelayCommand ClickHello { 
        //    get 
        //    {
        //        if (_clickHello == null)
        //        {
        //            _clickHello = new RelayCommand(Click);
        //        }
        //        return _clickHello;
        //    }
        //}

        public void Drop(object? sender, DragEventArgs e)
        {
            if (!e.Data.Contains("ChessMoveSourceSquare"))
                return;

            var chessMoveSourceSquare = e.Data.Get("ChessMoveSourceSquare") as string;
            if (chessMoveSourceSquare == null)
                return;

            Log += chessMoveSourceSquare + "\n";
        }

        //private RelayCommand _clickHello = null;
        private void Click()
        {
            ClickedCount++;

            // update the board to a random position
            var fens = new string[]
            {
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "r1b2rk1/4nppp/p3p3/2qpP3/8/2N2N2/PP3PPP/2RQ1RK1 b - - 3 14",
            "5n2/R7/4pk2/8/5PK1/8/8/8 b - - 0 1",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
            };
            var r = new Random(DateTime.Now.Millisecond);
            var fen = fens[r.Next(fens.Length)];
            ChessBoardViewModel.Position = BoardParsing.PositionFromFen(fen);
        }
    }
}
