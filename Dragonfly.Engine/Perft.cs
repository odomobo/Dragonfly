using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.PerformanceTypes;

namespace Dragonfly.Engine
{
    public sealed class Perft
    {
        private class PerftTable
        {
            private readonly struct Entry
            {
                public readonly ulong ZobristHash;
                public readonly int GamePly;
                public readonly int Count;

                public Entry(ulong zobristHash, int gamePly, int count)
                {
                    ZobristHash = zobristHash;
                    GamePly = gamePly;
                    Count = count;
                }
            }

            private readonly Entry[] _table;

            public PerftTable(int size)
            {
                _table = new Entry[size];
            }

            public bool TryGetEntry(ulong zobristHash, int gamePly, out int count)
            {
                var entry = _table[GetIndex(zobristHash)];
                if (entry.ZobristHash == zobristHash && entry.GamePly == gamePly)
                {
                    count = entry.Count;
                    return true;
                }
                else
                {
                    count = 0;
                    return false;
                }
            }

            public void SetEntry(ulong zobristHash, int gamePly, int count)
            {
                _table[GetIndex(zobristHash)] = new Entry(zobristHash, gamePly, count);
            }

            private int GetIndex(ulong zobristHash)
            {
                return (int)(zobristHash % (ulong)_table.Length);
            }
        }

        private readonly IMoveGen _moveGen;
        private readonly PerftTable? _perftTable;
        private readonly ObjectCacheByDepth<Position> _positionCache = new ObjectCacheByDepth<Position>();
        private readonly ObjectCacheByDepth<List<Move>> _moveListCache = new ObjectCacheByDepth<List<Move>>();

        public Perft(IMoveGen moveGen, int? tableSize = null)
        {
            _moveGen = moveGen;
            if (tableSize != null)
                _perftTable = new PerftTable(tableSize.Value);
            else
                _perftTable = null;
        }

        public int GoPerft(Position b, int depth)
        {
            if (depth <= 0)
                return 1;

            var ret = 0;

            if (_perftTable != null && _perftTable.TryGetEntry(b.ZobristHash, b.GamePly, out int count))
            {
                return count;
            }

            List<Move> moves = _moveListCache.Get(depth);
            moves.Clear();

            _moveGen.Generate(moves, b);

            var tmpPosition = _positionCache.Get(depth);
            foreach (var move in moves)
            {
                var nextPosition = Position.MakeMove(tmpPosition, move, b);
                
                // check move legality if using a pseudolegal move generator
                if (!_moveGen.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                    continue;

                ret += GoPerft(nextPosition, depth - 1);
            }

            if (_perftTable != null)
                _perftTable.SetEntry(b.ZobristHash, b.GamePly, ret);

            return ret;
        }
    }
}
