using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.TimeStrategies;

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
                    case "isready":
                        Console.WriteLine("readyok");
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
            var timeStrategy = new TimePerMoveStrategy(TimeSpan.FromSeconds(1));

            var (move, statistics) = _search.Search(_position, timeStrategy);
            PrintInfo(statistics);
            // TODO: print statistics
            Console.WriteLine($"bestmove {BoardParsing.CoordinateStringFromMove(move)}");
        }

        private static void PrintInfo(Statistics statistics)
        {
            var timeMs = (DateTime.Now - statistics.StartTime).TotalMilliseconds;
            var nps = (int)((statistics.Nodes / (double)timeMs) * 1000);
            var pvMoves = statistics.BestLine.Select(BoardParsing.CoordinateStringFromMove);
            var pvString = string.Join(' ', pvMoves);
            Console.WriteLine(
                $"info depth {statistics.CurrentDepth} " +
                $"seldepth {statistics.MaxPly} " +
                $"time {(int)timeMs} " +
                $"nodes {statistics.Nodes} " +
                $"nps {nps} " +
                $"pv {pvString} " +
                // TODO: add things like best move, score, etc
                "string " + // below this are nonstandard info values; I think without string, some GUIs would have a problem with this
                $"internalCutNodes {statistics.InternalCutNodes} " +
                $"internalPvNodes {statistics.InternalPVNodes} " +
                $"internalAllNodes {statistics.InternalAllNodes} " +
                $"qsearchCutNodes {statistics.QSearchCutNodes} " +
                $"qsearchPvNodes {statistics.QSearchPVNodes} " +
                $"qsearchAllNodes {statistics.QSearchAllNodes} " +
                $"evaluations {statistics.Evaluations} " +
                $"terminalNodes {statistics.TerminalNodes}"
            );
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
                string fen = "";
                options.RemoveAt(0);
                while (options.Count > 0 && options[0] != "moves")
                {
                    fen += $"{options[0]} ";
                    options.RemoveAt(0);
                }

                _position = BoardParsing.PositionFromFen(fen);
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
