using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Evaluation;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.MoveOrdering;
using Dragonfly.Engine.PerformanceTypes;
using Dragonfly.Engine.Searching;
using Dragonfly.Engine.TimeStrategies;

namespace Dragonfly
{
    static class Program
    {
        // TODO: where should this live?
        private const string OpeningFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string MidgameFen = "r1b2rk1/4nppp/p3p3/2qpP3/8/2N2N2/PP3PPP/2RQ1RK1 b - - 3 14";
        private const string EndgameFen = "5n2/R7/4pk2/8/5PK1/8/8/8 b - - 0 1";
        private const string KiwipeteFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";

        static void Main(string[] args)
        {
            // TODO: refactor test code out of here and into its own binary? maybe
            //Uci();
            Bench();
            //GetPerftPositions.DumpEvalsForEachPosition();
            //PerformanceTesting(OpeningFen, 5, TimeSpan.FromSeconds(10));
            //IncrementalPerft(KiwipeteFen, 7);
            //DivideTesting("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 5, "a1b1", "h3g2", "a2a3", "g2h1q");
        }

        private static void Uci()
        {
            var moveGen = new MoveGenerator();
            var evaluator = new Evaluator();
            var promotionMvvLvaMoveOrderer = new CompositeMoveOrderer(new IMoveOrderer[] {new PromotionsOrderer(), new MvvLvaOrderer()});
            var qSearch = new SimpleQSearch(evaluator, moveGen, promotionMvvLvaMoveOrderer, CompositeMoveOrderer.NullMoveOrderer);
            //var search = new SimpleAlphaBetaSearch(moveGen, evaluator, qSearch);
            var search = new IidAlphaBetaSearch(moveGen, new StandardGameTerminator(),  evaluator, qSearch, promotionMvvLvaMoveOrderer);

            var uci = new SimpleUci(moveGen, search, Console.In, TextWriter.Synchronized(Console.Out));
            uci.Loop();
        }

        private static void Bench()
        {
            var moveGen = new MoveGenerator();
            var evaluator = new Evaluator();
            var promotionMvvLvaMoveOrderer = new CompositeMoveOrderer(new IMoveOrderer[] { new PromotionsOrderer(), new MvvLvaOrderer() });
            var qSearch = new SimpleQSearch(evaluator, moveGen, promotionMvvLvaMoveOrderer, CompositeMoveOrderer.NullMoveOrderer);
            var search = new SimpleAlphaBetaSearch(moveGen, new StandardGameTerminator(), evaluator, qSearch);
            var timeStrategy = new TimePerMoveStrategy(TimeSpan.FromSeconds(10));

            search.Search(BoardParsing.PositionFromFen(MidgameFen), timeStrategy, new DummyProtocol());
        }

        private static void PerformanceTesting(string fen, int perftDepth, TimeSpan timespan)
        {
            var position = BoardParsing.PositionFromFen(fen);
            Debugging.Dump(position);

            var moveGen = new MoveGenerator();
            var perft = new Perft(moveGen);
            Stopwatch overallStopwatch = new Stopwatch();
            overallStopwatch.Start();
            while (true)
            {
                int perftNum = perftDepth;
                Console.Write($"Perft {perftNum}: ");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                int perftResults = perft.GoPerft(position, perftNum);
                sw.Stop();
                double knps = ((double) perftResults) / sw.ElapsedMilliseconds; // it just works out
                Console.WriteLine($"{perftResults} ({knps:F2} knps)");
                if (overallStopwatch.Elapsed > timespan)
                    break;
            }

            //perft.GoPerft(position, perftDepth);
        }

        private static void IncrementalPerft(string fen, int maxDepth)
        {
            var position = BoardParsing.PositionFromFen(fen);
            Debugging.Dump(position);

            var moveGen = new MoveGenerator();
            var perft = new Perft(moveGen);
            for (int i = 1; i <= maxDepth; i++)
            {
                Console.Write($"Perft {i}: ");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                int perftResults = perft.GoPerft(position, i);
                sw.Stop();
                double knps = ((double) perftResults) / sw.ElapsedMilliseconds; // it just works out
                Console.WriteLine($"{perftResults} ({knps:F2} knps)");
            }
        }

        private static void DivideTesting(string fen, int depth, params string[] moves)
        {
            var position = BoardParsing.PositionFromFen(fen);
            Debugging.Dump(position);

            var moveGen = new MoveGenerator();
            var perft = new Perft(moveGen);
            
            foreach (var moveStr in moves)
            {
                Move move = BoardParsing.GetMoveFromCoordinateString(moveGen, position, moveStr);
                position = position.MakeMove(move);
                Debugging.Dump(position);
            }

            GoDivide(moveGen, perft, position, depth - moves.Length);
        }

        private static void GoDivide(MoveGenerator moveGenerator, Perft perft, Position position, int depth)
        {
            if (depth <= 0)
            {
                Console.WriteLine($"##### No moves generated at depth {depth}");
                return;
            }

            var total = 0;

            var moves = new List<Move>();
            moveGenerator.Generate(moves, position);

            var movesDict = new SortedDictionary<string, Move>();
            foreach (var move in moves)
            {
                movesDict.Add(BoardParsing.CoordinateStringFromMove(move), move);
            }

            foreach (var (moveStr, move) in movesDict)
            {
                var nextBoard = position.MakeMove(move);

                // check move legality if using a pseudolegal move generator
                if (!moveGenerator.OnlyLegalMoves && nextBoard.MovedIntoCheck())
                    continue;

                Console.Write($"{moveStr}: ");

                int count = perft.GoPerft(nextBoard, depth - 1);
                Console.WriteLine(count);
                total += count;
            }
            Console.WriteLine($"##### Total moves: {total}");
        }
    }
}
