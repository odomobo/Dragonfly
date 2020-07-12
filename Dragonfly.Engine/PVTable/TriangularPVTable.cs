using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.PVTable
{
    public sealed class TriangularPVTable : IPVTable
    {
        private readonly List<List<Move>> _table;
        private readonly List<Move> _bestLine;

        public TriangularPVTable()
        {
            _table = new List<List<Move>>();
            _bestLine = new List<Move>();
        }

        public void Add(Move move, int ply)
        {
            // We need to have at least added a move from the previous ply.
            // Even if this assertion isn't hit, we will get an exception below.
            Debug.Assert(ply < _table.Count + 1);

            if (_table.Count <= ply)
                _table.Add(new List<Move>());

            var moveList = _table[ply];
            moveList.Clear();
            moveList.Add(move);
        }

        public void Commit(int ply)
        {
            if (ply == 0)
            {
                _bestLine.Clear();
                foreach (var move in _table[0])
                    _bestLine.Add(move);
            }
            else
            {
                Debug.Assert(ply < _table.Count);

                // TODO: rename
                var baseMove = _table[ply - 1];
                var newMoves = _table[ply];

                // remove all but the first element
                baseMove.RemoveRange(1, baseMove.Count - 1);

                foreach (var move in newMoves)
                    baseMove.Add(move);
            }
        }

        public List<Move> GetBestLine()
        {
            return _bestLine.ToList();
        }

        public Move GetCurrentMove()
        {
            if (_table.Count > 0)
                return _table[0][0];
            else
                return Move.Null;
        }
    }
}
