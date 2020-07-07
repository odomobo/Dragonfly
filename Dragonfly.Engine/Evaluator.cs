using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine
{
    class Evaluator : IEvaluator
    {
        private static readonly short[] Pawn = {
             0,  0,  0,  0,  0,  0,  0,  0,
             5, 10, 10,-20,-20, 10, 10,  5,
             5, -5,-10,  0,  0,-10, -5,  5,
             0,  0,  0, 20, 20,  0,  0,  0,
             5,  5, 10, 25, 25, 10,  5,  5,
            10, 10, 20, 30, 30, 20, 10, 10,
            50, 50, 50, 50, 50, 50, 50, 50,
             0,  0,  0,  0,  0,  0,  0,  0,
        };

        private static readonly short[] Knight = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        private static readonly short[] Bishop = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };

        private static readonly short[] Rook = {
              0,  0,  0,  5,  5,  0,  0,  0,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              5, 10, 10, 10, 10, 10, 10,  5,
              0,  0,  0,  0,  0,  0,  0,  0,
        };

        private static readonly short[] Queen = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -10,  5,  5,  5,  5,  5,  0,-10,
              0,  0,  5,  5,  5,  5,  0, -5,
             -5,  0,  5,  5,  5,  5,  0, -5,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20,
        };

        // taken from Simplified Evaluation function, but normalized such that a castled king has a value of 0 (for easy transition into endgame evaluation)
        private static readonly short[] KingMiddleGame = {
            -10,  0,-20,-30,-30,-20,  0,-10,
            -10,-10,-30,-30,-30,-30,-10,-10,
            -40,-50,-50,-50,-50,-50,-50,-40,
            -50,-60,-60,-70,-70,-60,-60,-50,
            -60,-70,-70,-80,-80,-70,-70,-60,
            -60,-70,-70,-80,-80,-70,-70,-60,
            -60,-70,-70,-80,-80,-70,-70,-60,
            -60,-70,-70,-80,-80,-70,-70,-60,
        };

        // taken from vice's king endgame evaluation; a castled king has a value of -10, and a mostly-castled king has a value of 0
        private static readonly short[] KingEndGame = {
            -50,-10,  0,  0,  0,  0,-10,-50,
            -10,  0, 10, 10, 10, 10,  0,-10,
              0, 10, 20, 20, 20, 20, 10,  0,
              0, 10, 20, 40, 40, 20, 10,  0,
              0, 10, 20, 40, 40, 20, 10,  0,
              0, 10, 20, 20, 20, 20, 10,  0,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -50,-10,  0,  0,  0,  0,-10,-50,
        };

        public int Evaluate(Board board)
        {
            throw new NotImplementedException();
        }
    }
}
