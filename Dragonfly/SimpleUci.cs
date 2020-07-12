﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private ITimeStrategy _timeStrategy;
        private Thread? _searchThread;

        public SimpleUci(IMoveGen moveGen, ISearch search)
        {
            _position = BoardParsing.PositionFromFen(InitialFen);
            _moveGen = moveGen;
            _search = search;
            _timeStrategy = new DefaultTimeStrategy();
        }

        public void Loop()
        {
            while (true)
            {
                var line = Console.ReadLine();
                var splitLine = line.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);
                var command = splitLine[0];
                var options = splitLine.Length > 1 ? splitLine[1] : string.Empty;

                // TODO: synchonize
                switch (command)
                {
                    case "quit":
                        InterruptSearch();
                        return;
                    case "stop":
                        InterruptSearch();
                        break;
                    case "uci":
                        WaitForSearch();
                        HandleUci();
                        break;
                    case "isready":
                        WaitForSearch();
                        Console.WriteLine("readyok");
                        break;
                    case "position":
                        WaitForSearch();
                        SetPosition(options);
                        break;
                    case "go":
                        WaitForSearch();
                        _timeStrategy = TimeStrategyFromGoOptionsDict(OptionsDictFromGoOptions(options));
                        _searchThread = new Thread(Go);
                        _searchThread.Start();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        break;
                }
            }
        }

        private void InterruptSearch()
        {
            if (_searchThread == null)
                return;

            _timeStrategy.ForceStop();
            _searchThread.Join();
            _searchThread = null;
        }

        private void WaitForSearch()
        {
            if (_searchThread == null)
                return;

            _searchThread.Join();
            _searchThread = null;
        }

        private static void HandleUci()
        {
            // Note: this should ideally match what is in the project properties
            Console.WriteLine("id name Dragonfly 0.1.0.0");
            Console.WriteLine("id author odomobo");

            // TODO: options

            Console.WriteLine("uciok");
        }

        private void Go()
        {
            var (move, statistics) = _search.Search(_position, _timeStrategy);
            PrintInfo(statistics);
            // TODO: print statistics
            Console.WriteLine($"bestmove {BoardParsing.CoordinateStringFromMove(move)}");
        }

        #region Go options

        private static readonly HashSet<string> GoOptionsKeys = new HashSet<string>(new []{
            "searchmoves",
            "ponder",
            "wtime",
            "btime",
            "winc",
            "binc",
            "movestogo",
            "depth",
            "nodes",
            "mate",
            "movetime",
            "infinite"
        }, StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, List<string>> OptionsDictFromGoOptions(string options)
        {
            var ret = new Dictionary<string, List<string>>();
            var splitOptions = options.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<string> currentOptionParameters = new List<string>();
            foreach (var option in splitOptions)
            {
                if (GoOptionsKeys.Contains(option))
                {
                    currentOptionParameters = new List<string>();
                    ret[option] = currentOptionParameters;
                }
                else
                {
                    currentOptionParameters.Add(option);
                }
            }

            return ret;
        }

        /// <summary>
        /// If there are no applicable options for controlling the time strategy, then we default to a basic infinite strategy.
        /// If there is only one applicable option (or set of options in the case of wtime, winc, btime, binc, movestogo),
        /// then we use that option to build a time strategy.
        /// Finally, if there are multiple options, then make a composite strategy with all the other time strategies.
        /// This will exit if any of the time strategies exits.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private ITimeStrategy TimeStrategyFromGoOptionsDict(Dictionary<string, List<string>> options)
        {
            List<ITimeStrategy> timeStrategies = new List<ITimeStrategy>();

            // TODO: don't throw exception if options aren't available
            if (options.TryGetValue("depth", out var depthParameters))
            {
                int depth = int.Parse(depthParameters[0]);
                timeStrategies.Add(new DepthStrategy(depth));
            }

            if (options.TryGetValue("nodes", out var nodesParameters))
            {
                int nodes = int.Parse(nodesParameters[0]);
                timeStrategies.Add(new NodeCountStrategy(nodes));
            }

            if (options.TryGetValue("movetime", out var movetimeParameters))
            {
                int movetimeMs = int.Parse(movetimeParameters[0]);
                timeStrategies.Add(new TimePerMoveStrategy(TimeSpan.FromMilliseconds(movetimeMs)));
            }

            // TODO: add a special case for infinite

            Color sideToCalculate = _position.SideToMove;
            if ((sideToCalculate == Color.White && options.ContainsKey("wtime")) ||
                (sideToCalculate == Color.Black && options.ContainsKey("btime")))
            {
                TimeSpan? wtime = null;
                if (options.TryGetValue("wtime", out var wtimeParameters))
                    wtime = TimeSpan.FromMilliseconds(int.Parse(wtimeParameters[0]));

                TimeSpan? btime = null;
                if (options.TryGetValue("btime", out var btimeParameters))
                    btime = TimeSpan.FromMilliseconds(int.Parse(btimeParameters[0]));

                TimeSpan? winc = null;
                if (options.TryGetValue("winc", out var wincParameters))
                    winc = TimeSpan.FromMilliseconds(int.Parse(wincParameters[0]));

                TimeSpan? binc = null;
                if (options.TryGetValue("binc", out var bincParameters))
                    binc = TimeSpan.FromMilliseconds(int.Parse(bincParameters[0]));

                int? movesToGo = null;
                if (options.TryGetValue("movestogo", out var movesToGoParameters))
                    movesToGo = int.Parse(movesToGoParameters[0]);

                if (sideToCalculate == Color.White)
                {
                    timeStrategies.Add(new ClockTimeStrategy(wtime.Value, winc, btime, binc, movesToGo));
                }
                else
                {
                    timeStrategies.Add(new ClockTimeStrategy(btime.Value, binc, wtime, winc, movesToGo));
                }
            }

            if (timeStrategies.Count == 0)
                return new DefaultTimeStrategy();
            else if (timeStrategies.Count == 0)
                return timeStrategies.First();
            else
                return new CompositeTimeStrategy(timeStrategies);
        }

        #endregion Go options

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
                $"score cp {statistics.CurrentScore.Value} " + // TODO: make this more sophisticated
                $"pv {pvString} " +
                // TODO: add things like best move, score, etc
                "string " + // below this are nonstandard info values; I think without string, some GUIs would have a problem with this
                $"internalCutNodes {statistics.InternalCutNodes} " +
                $"internalPvNodes {statistics.InternalPVNodes} " +
                $"internalAllNodes {statistics.InternalAllNodes} " +
                $"internalMovesEvaluated {statistics.InternalMovesEvaluated} " +
                $"internalBranchingFactor {statistics.InternalBranchingFactor:F2} " +
                $"internalAverageCutMoveMisses {statistics.InternalAverageCutMoveMisses:F2} " +
                $"qsearchCutNodes {statistics.QSearchCutNodes} " +
                $"qsearchPvNodes {statistics.QSearchPVNodes} " +
                $"qsearchAllNodes {statistics.QSearchAllNodes} " +
                $"qsearchMovesEvaluated {statistics.QSearchMovesEvaluated} " +
                $"qsearchBranchingFactor {statistics.QSearchBranchingFactor:F2} " +
                $"qsearchAverageCutMoveMisses {statistics.QSearchAverageCutMoveMisses:F2} " +
                $"evaluations {statistics.Evaluations} " +
                $"terminalNodes {statistics.TerminalNodes} "
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
