using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface IGameTerminator
    {
        bool IsPositionTerminal(Position position, out Score score);

        /// <summary>
        /// This will differentiate between checkmate and stalemate
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Score NoLegalMovesScore(Position position);
    }
}
