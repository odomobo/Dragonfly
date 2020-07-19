using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.Evaluation
{
    // This is supposed to make highly speculative moves. It shouldn't be very strong.
    public sealed class CrazyEvaluator : IEvaluator
    {
        private static readonly Score Tempo = 20;

        private static readonly Score[] MidgamePieceSquareTables =
        {
            // Pawn
              0,  0,  0,  0,  0,  0,  0,  0,
              5, 10, 10,-20,-20, 10, 10,  5,
              5, -5,-10,  0,  0,-10, -5,  5,
              0,  0,  0, 20, 20,  0,  0,  0,
              5,  5, 10, 25, 25, 10,  5,  5,
             10, 10, 20, 30, 30, 20, 10, 10,
             50, 50, 50, 50, 50, 50, 50, 50,
              0,  0,  0,  0,  0,  0,  0,  0,
            // Knight
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
            // Bishop
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
            // Rook
              0,  0,  0,  5,  5,  0,  0,  0,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              5, 10, 10, 10, 10, 10, 10,  5,
              0,  0,  0,  0,  0,  0,  0,  0,
            // Queen
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -10,  5,  5,  5,  5,  5,  0,-10,
              0,  0,  5,  5,  5,  5,  0, -5,
             -5,  0,  5,  5,  5,  5,  0, -5,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20,
            // King
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
        // TODO: need to use this
        private static readonly Score[] KingEndGame = {
            -50,-10,  0,  0,  0,  0,-10,-50,
            -10,  0, 10, 10, 10, 10,  0,-10,
              0, 10, 20, 20, 20, 20, 10,  0,
              0, 10, 20, 40, 40, 20, 10,  0,
              0, 10, 20, 40, 40, 20, 10,  0,
              0, 10, 20, 20, 20, 20, 10,  0,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -50,-10,  0,  0,  0,  0,-10,-50,
        };

        // this needs to match the values of PieceType
        private static readonly Score[] MaterialValues =
        {
            100, // pawn
            320, // knight
            330, // bishop
            500, // rook
            900, // queen
            20000// king
        };

        // incredibly naive evaluator; only measures material, pst, and tempo. Doesn't have endgame eval
        public Score Evaluate(Position position)
        {
            Span<Score> midgamePstValues = stackalloc Score[2];
            Span<Score> materialValues = stackalloc Score[2];

            for (Color color = 0; color < (Color) 2; color++)
            {
                for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
                {
                    var piecePositions = position.GetPieceBitboard(color, pieceType);

                    materialValues[(int) color] += (MaterialValues[(int) pieceType] * Bits.PopCount(piecePositions));
                    while (Bits.TryPopLsb(ref piecePositions, out int pieceIx))
                    {
                        midgamePstValues[(int) color] += MidgamePieceSquareTables[GetPstIndex(color, pieceType, pieceIx)];
                    }
                }
            }

            // How to do this efficiently without making a bunch of objects, and allowing each evaluator to have its own parameters?

            // the core of the speculation is valuing positioning 10x more highly than it should
            var ret = midgamePstValues[(int) Color.White]*10 + materialValues[(int) Color.White]
                      - midgamePstValues[(int) Color.Black]*10 - materialValues[(int) Color.Black];

            if (position.SideToMove == Color.White)
                ret += Tempo;
            else
                ret -= Tempo;

            return ret;
        }

        public static int GetPstIndex(Color color, PieceType pieceType, int boardIndex)
        {
            return (int)pieceType * 64 + IxForColor(boardIndex, color);
        }

        public static int IxForColor(int ix, Color color)
        {
            // Swap rank but not file... somehow this works out.
            // Swapper is 0 for white, or 0b111000 for black.
            // Xor with 0b111000 will invert rank, which is the same as swapping it
            int swapper = (0b111000 * (int) color);
            return ix ^ swapper;
        }
    }
}
