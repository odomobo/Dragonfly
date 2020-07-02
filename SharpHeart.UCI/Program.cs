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

            for (int i = 1; i <= 6; i++)
            {
                Console.Write($"Perft {i}: ");
                
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int perftResults = perft.GoPerft(board, i);
                sw.Stop();
                double knps = ((double)perftResults) / sw.ElapsedMilliseconds; // it just works out
                Console.WriteLine($"{perftResults} ({knps:F2} knps)");
            }
            

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
