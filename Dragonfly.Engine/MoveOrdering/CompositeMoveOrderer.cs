using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.MoveOrdering
{
    // TODO: name something else?? this isn't an IMoveOrderer
    public sealed class CompositeMoveOrderer
    {
        public static readonly CompositeMoveOrderer NullMoveOrderer = new CompositeMoveOrderer(new IMoveOrderer[]{});

        private readonly IMoveOrderer[] _moveOrderers;

        public CompositeMoveOrderer(IEnumerable<IMoveOrderer> moveOrderers)
        {
            _moveOrderers = moveOrderers.ToArray();
        }

        public IEnumerable<Move> Order(List<Move> moves, Position position)
        {
            int start = 0;
            int count = moves.Count;

            if (moves.Count > 0 && _moveOrderers.Length > 0)
            {
                int foo = 0;
            }

            foreach (var moveOrderer in _moveOrderers)
            {
                var pivot = moveOrderer.PartitionAndSort(moves, start, count, position);
                for (; start < pivot; start++)
                    yield return moves[start];

                // calculate remaining count
                count = moves.Count - start;
            }

            // return all of the stragglers (not matched by anything)
            for (; start < moves.Count; start++)
            {
                yield return moves[start];
            }
        }
    }
}
