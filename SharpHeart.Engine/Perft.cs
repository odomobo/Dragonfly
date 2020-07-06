using System;
using System.Collections.Generic;
using System.Text;
using SharpHeart.Engine.Interfaces;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.Engine
{
    public class Perft
    {
        private readonly IMoveGen _moveGen;
        public Perft(IMoveGen moveGen)
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
                var nextBoard = b.DoMove(move);
                
                // check move legality if using a pseudolegal move generator
                if (!_moveGen.OnlyLegalMoves && nextBoard.InCheck(nextBoard.SideToMove.Other()))
                    continue;

                ret += GoPerft(nextBoard, depth - 1);
            }

            return ret;
        }
    }
}
