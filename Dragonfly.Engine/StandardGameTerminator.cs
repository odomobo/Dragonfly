using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine
{
    public class StandardGameTerminator : IGameTerminator
    {
        public bool IsPositionTerminal(Position position, out Score score)
        {
            // Note: we don't return draw on 50 move counter; if the engine doesn't know how to make progress in 50 moves, telling the engine it's about to draw can only induce mistakes.
            if (position.RepetitionNumber >= 3)
            {
                // TODO: contempt
                score = 0; // draw
                return true;
            }
            else
            {
                score = default;
                return false;
            }
        }

        public Score NoLegalMovesScore(Position position)
        {
            if (position.InCheck())
                return Score.GetMateScore(position.GamePly);
            else
                return 0; // draw; TODO: contempt
        }
    }
}
