using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface IMoveGenerator
    {
        bool OnlyLegalMoves { get; }
        void Generate(List<Move> moves, Position position);
    }
}
