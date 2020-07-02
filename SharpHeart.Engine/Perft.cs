using System;
using System.Collections.Generic;
using System.Text;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.Engine
{
    public class Perft
    {
        private readonly MoveGen _moveGen;
        public Perft(MoveGen moveGen)
        {
            _moveGen = moveGen;
        }

        public int GoPerft(Board b, int depth)
        {
            if (depth <= 0)
                return 1;

            var ret = 0;

            List<Move> moves = new List<Move>();
            _moveGen.Generate(moves, b);

            foreach (var move in moves)
            {
                var nextBoard = move.DoMove(b);
                
                // if we moved into check, clearly it was an invalid move
                if (nextBoard.InCheck(nextBoard.SideToMove.Other()))
                    continue;

                ret += GoPerft(nextBoard, depth - 1);
            }

            return ret;
        }
    }
}
