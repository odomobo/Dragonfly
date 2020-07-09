using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;

namespace Dragonfly
{
    class SimpleUci
    {
        private const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private Position _position;
        private IMoveGen _moveGen;
        private ISearch _search;

        public SimpleUci(IMoveGen moveGen, ISearch search)
        {
            _position = BoardParsing.PositionFromFen(InitialFen);
            _moveGen = moveGen;
            _search = search;
        }

        public void Loop()
        {
            while (true)
            {
                var line = Console.ReadLine();
                var splitLine = line.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);
                var command = splitLine[0];
                var options = splitLine.Length > 1 ? splitLine[1] : string.Empty;

                switch (command)
                {
                    case "quit":
                        return;
                    case "uci":
                        Console.WriteLine("uciok");
                        break;
                    case "position":
                        SetPosition(options);
                        break;
                    case "go":
                        Go();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        break;
                }
            }
        }

        private void Go()
        {
            var move = _search.Search(_position);
            Console.WriteLine(BoardParsing.CoordinateStringFromMove(move));
        }

        private void SetPosition(string optionsStr)
        {
            var options = optionsStr.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (options[0] == "startpos")
            {
                _position = BoardParsing.PositionFromFen(InitialFen);
                options.RemoveAt(0);
            }
            else if (options[0] == "fen")
            {
                _position = BoardParsing.PositionFromFen(options[1]);
                options.RemoveAt(0);
                options.RemoveAt(0);
            }
            else
            {
                throw new Exception($"Invalid option: {options[0]}");
            }

            if (options.Any() && options[0] == "moves")
            {
                options.RemoveAt(0);

                foreach (var moveStr in options)
                {
                    Move move = BoardParsing.GetMoveFromCoordinateString(_moveGen, _position, moveStr);
                    _position = Position.MakeMove(new Position(), move, _position);
                }
            }
        }
    }
}
