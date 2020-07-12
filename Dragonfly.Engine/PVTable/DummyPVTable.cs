using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.PVTable
{
    public sealed class DummyPVTable : IPVTable
    {
        public void Add(Move move, int ply)
        {}

        public void Commit(int ply)
        {}

        public List<Move> GetBestLine()
        {
            return new List<Move>();
        }

        public Move GetCurrentMove()
        {
            return Move.Null;
        }
    }
}
