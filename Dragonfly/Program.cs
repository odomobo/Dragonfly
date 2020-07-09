using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Evaluation;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.Searching;

namespace Dragonfly
{
    class Program
    {
        // TODO: where should this live?
        private const string OpeningFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string MidgameFen = "r1b2rk1/4nppp/p3p3/2qpP3/8/2N2N2/PP3PPP/2RQ1RK1 b - - 3 14";
        private const string EndgameFen = "5n2/R7/4pk2/8/5PK1/8/8/8 b - - 0 1";
        private const string KiwipeteFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
        static void Main(string[] args)
        {
            Uci();
            //Bench();
            //GetPerftPositions.DumpEvalsForEachPosition();
            //PerformanceTesting(OpeningFen, 5, TimeSpan.FromSeconds(10));
            //IncrementalPerft(KiwipeteFen, 7);
            //DivideTesting("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", 5, "a1b1", "h3g2", "a2a3", "g2h1q");
        }

        private static void Uci()
        {
            var moveGen = new MoveGen();
            var evaluator = new Evaluator();
            var search = new SimpleAlphaBetaSearch(moveGen, evaluator);

            var uci = new SimpleUci(moveGen, search);
            uci.Loop();
        }

        private static void Bench()
        {
            var moveGen = new MoveGen();
            var evaluator = new Evaluator();
            var search = new SimpleAlphaBetaSearch(moveGen, evaluator);

            search.Search(BoardParsing.PositionFromFen(OpeningFen));
        }

        private static void PerformanceTesting(string fen, int perftDepth, TimeSpan timespan)
        {
            var board = BoardParsing.PositionFromFen(fen);
            Debugging.Dump(board);

            var moveGen = new MoveGen();
            var perft = new Perft(moveGen);
            Stopwatch overallStopwatch = new Stopwatch();
            overallStopwatch.Start();
            while (true)
            {
                int perftNum = perftDepth;
                Console.Write($"Perft {perftNum}: ");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                int perftResults = perft.GoPerft(board, perftNum);
                sw.Stop();
                double knps = ((double) perftResults) / sw.ElapsedMilliseconds; // it just works out
                Console.WriteLine($"{perftResults} ({knps:F2} knps)");
                if (overallStopwatch.Elapsed > timespan)
                    break;
            }

            //perft.GoPerft(board, perftDepth);
        }

        private static void IncrementalPerft(string fen, int maxDepth)
        {
            var board = BoardParsing.PositionFromFen(fen);
            Debugging.Dump(board);

            var moveGen = new MoveGen();
            var perft = new Perft(moveGen);
            for (int i = 1; i <= maxDepth; i++)
            {
                Console.Write($"Perft {i}: ");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                int perftResults = perft.GoPerft(board, i);
                sw.Stop();
                double knps = ((double) perftResults) / sw.ElapsedMilliseconds; // it just works out
                Console.WriteLine($"{perftResults} ({knps:F2} knps)");
            }
        }

        private static void DivideTesting(string fen, int depth, params string[] moves)
        {
            var board = BoardParsing.PositionFromFen(fen);
            Debugging.Dump(board);

            var moveGen = new MoveGen();
            var perft = new Perft(moveGen);
            
            foreach (var moveStr in moves)
            {
                Move move = BoardParsing.GetMoveFromCoordinateString(moveGen, board, moveStr);
                board = Position.MakeMove(new Position(), move, board);
                Debugging.Dump(board);
            }

            GoDivide(moveGen, perft, board, depth - moves.Length);
        }

        private static void GoDivide(MoveGen moveGen, Perft perft, Position position, int depth)
        {
            if (depth <= 0)
            {
                Console.WriteLine($"##### No moves generated at depth {depth}");
                return;
            }

            var total = 0;

            List<Move> moves = new List<Move>();
            moveGen.Generate(moves, position);

            var movesDict = new SortedDictionary<string, Move>();
            foreach (var move in moves)
            {
                movesDict.Add(BoardParsing.CoordinateStringFromMove(move), move);
            }

            foreach (var (moveStr, move) in movesDict)
            {
                var nextBoard = Position.MakeMove(new Position(), move, position);

                // check move legality if using a pseudolegal move generator
                if (!moveGen.OnlyLegalMoves && nextBoard.InCheck(nextBoard.SideToMove.Other()))
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
