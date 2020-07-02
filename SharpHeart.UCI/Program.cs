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
        private const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string TestingFen = "r1b2rk1/4nppp/p3p3/2qpP3/8/2N2N2/PP3PPP/2RQ1RK1 b - - 3 14";
        static void Main(string[] args)
        {
            var board = BoardParsing.FromFen(InitialFen);
            Debugging.Dump(board);

            var moveGen = new MoveGen();
            var perft = new Perft(moveGen);
            perft.GoDivide(board, 5);

            Move move = moveGen.GetMoveFromCoordinateString(board, "d2d3");
            board = move.DoMove(board);
            Debugging.Dump(board);
            perft.GoDivide(board, 4);

            move = moveGen.GetMoveFromCoordinateString(board, "a7a5");
            board = move.DoMove(board);
            Debugging.Dump(board);
            perft.GoDivide(board, 3);

            move = moveGen.GetMoveFromCoordinateString(board, "e1d2");
            board = move.DoMove(board);
            Debugging.Dump(board);
            perft.GoDivide(board, 2);

            move = moveGen.GetMoveFromCoordinateString(board, "a5a4");
            board = move.DoMove(board);
            Debugging.Dump(board);
            perft.GoDivide(board, 1);


            //for (int i = 1; i <= 6; i++)
            //{
            //    Console.Write($"Perft {i}: ");

            //    Stopwatch sw = new Stopwatch();
            //    sw.Start();
            //    int perftResults = perft.GoPerft(board, i);
            //    sw.Stop();
            //    double knps = ((double)perftResults) / sw.ElapsedMilliseconds; // it just works out
            //    Console.WriteLine($"{perftResults} ({knps:F2} knps)");
            //}


            //List<Move> moves = new List<Move>();
            //moveGen.Generate(moves, board);
            //Debugging.Dump(moves);

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //PawnQuietMoveTable.DumpMagics();
            //sw.Stop();
            //Console.WriteLine($"Elapsed time: {sw.Elapsed}");
        }
    }
}
