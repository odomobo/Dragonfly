using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.PerformanceTypes;

namespace Dragonfly.Engine.Searching
{
    public sealed class SimpleAlphaBetaSearch : ISearch
    {
        private readonly IMoveGen _moveGen;
        private readonly IEvaluator _evaluator;
        private readonly ObjectCacheByDepth<Position> _positionCache = new ObjectCacheByDepth<Position>();
        private readonly ObjectCacheByDepth<List<Move>> _moveListCache = new ObjectCacheByDepth<List<Move>>();

        public SimpleAlphaBetaSearch(IMoveGen moveGen, IEvaluator evaluator)
        {
            _moveGen = moveGen;
            _evaluator = evaluator;
        }

        public Move Search(Position position)
        {
            Move bestMove = Move.Null;
            var moveList = new List<Move>();
            _moveGen.Generate(moveList, position);

            // we need to be careful not to use short.MinValue, because we can't safely negate that
            short alpha = -32000;
            short beta = 32000;

            var cachedPositionObject = new Position();
            foreach (var move in moveList)
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                if (!_moveGen.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                    continue;

                // let's just naively do a depth 2 search (including this search as 1 depth)
                var nextEval = (short)-InnerSearch(nextPosition, 1, (short) -beta, (short) -alpha);

                if (nextEval > alpha)
                {
                    alpha = nextEval;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private short InnerSearch(Position position, int depth, short alpha, short beta)
        {
            // This is pretty bad; we really want to do quiescence search
            if (depth <= 0)
            {
                var eval = _evaluator.Evaluate(position);
                if (position.SideToMove == Color.White)
                    return eval;
                else
                    return (short)-eval;
            }

            var moveList = _moveListCache.Get(depth);
            _moveGen.Generate(moveList, position);

            bool anyMoves = false;

            var cachedPositionObject = _positionCache.Get(depth);
            foreach (var move in moveList)
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                if (!_moveGen.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                    continue;

                anyMoves = true;

                short nextEval = (short)-InnerSearch(nextPosition, depth - 1, (short)-beta, (short)-alpha);
                if (nextEval > beta)
                    return nextEval; // fail soft, but shouldn't matter for this naive implementation

                if (nextEval > alpha)
                {
                    // TODO eventually: we need to store this into triangular pv table somehow
                    alpha = nextEval;
                }
            }

            if (!anyMoves)
            {
                if (position.InCheck())
                    return 32000; // TODO: properly represent mate
                else
                    return 0; // draw
            }

            return alpha;
        }
    }
}
