using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpHeart.Engine;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.UCI
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
            //PerformanceTesting(OpeningFen, 5, TimeSpan.FromSeconds(10));
            //IncrementalPerft(KiwipeteFen, 7);
            DivideTesting("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 2, "a2a3");
        }

        private static void PerformanceTesting(string fen, int perftDepth, TimeSpan timespan)
        {
            var board = BoardParsing.BoardFromFen(fen);
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
            var board = BoardParsing.BoardFromFen(fen);
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
            var board = BoardParsing.BoardFromFen(fen);
            Debugging.Dump(board);

            var moveGen = new MoveGen();
            var perft = new Perft(moveGen);
            int total = perft.GoDivide(board, depth--);
            Console.WriteLine($"##### Total moves: {total}");

            foreach (var moveStr in moves)
            {
                Move move = moveGen.GetMoveFromCoordinateString(board, moveStr);
                board = board.DoMove(move);
                Debugging.Dump(board);
                total = perft.GoDivide(board, depth--);
                Console.WriteLine($"##### Total moves: {total}");
            }
        }
    }
}
