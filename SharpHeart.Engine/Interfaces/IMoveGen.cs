using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.Interfaces
{
    public interface IMoveGen
    {
        void Generate(ref List<Move> moves, Board board);
    }
}
