using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.Interfaces
{
    public interface IMoveGen
    {
        bool OnlyLegalMoves { get; }
        void Generate(List<Move> moves, Board board);
    }
}
