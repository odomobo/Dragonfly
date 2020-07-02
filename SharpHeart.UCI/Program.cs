using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpHeart.Engine;
using SharpHeart.Engine.MoveGen;

namespace SharpHeart.UCI
{
    class Program
    {
        // TODO: where should this live?
        private const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        static void Main(string[] args)
        {
            var board = BoardParsing.FromFen("r1b2rk1/4nppp/p3p3/2qpP3/8/2N2N2/PP3PPP/2RQ1RK1 b - - 3 14");
            BoardParsing.Dump(board);

            var moveGen = new MoveGen();

            List<Move> moves = new List<Move>();
            moveGen.Generate(moves, board);
            BoardParsing.Dump(moves);

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //PawnQuietMoveTable.DumpMagics();
            //sw.Stop();
            //Console.WriteLine($"Elapsed time: {sw.Elapsed}");
        }
    }
}
