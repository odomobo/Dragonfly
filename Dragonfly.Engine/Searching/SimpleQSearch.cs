using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveOrdering;
using Dragonfly.Engine.PerformanceTypes;

namespace Dragonfly.Engine.Searching
{
    public sealed class SimpleQSearch : IQSearch
    {
        private const int MaxDepth = 10;

        private readonly IEvaluator _evaluator;
        private readonly IMoveGenerator _moveGenerator; // Note: eventually we also want a qsearch movegen
        private readonly CompositeMoveOrderer _quiescenceMoveOrderer;
        private readonly CompositeMoveOrderer _checkEvasionsMoveOrderer;
        private ITimeStrategy _timeStrategy; // Note: we don't actually need this
        private Statistics _statistics;
        private readonly ObjectCacheByDepth<Position> _positionCache = new ObjectCacheByDepth<Position>();
        private readonly ObjectCacheByDepth<List<Move>> _moveListCache = new ObjectCacheByDepth<List<Move>>();

        public SimpleQSearch(IEvaluator evaluator, IMoveGenerator moveGenerator, CompositeMoveOrderer quiescenceMoveOrderer, CompositeMoveOrderer checkEvasionsMoveOrderer)
        {
            _evaluator = evaluator;
            _moveGenerator = moveGenerator;
            _quiescenceMoveOrderer = quiescenceMoveOrderer;
            _checkEvasionsMoveOrderer = checkEvasionsMoveOrderer;
        }

        public void StartSearch(ITimeStrategy timeStrategy, IPVTable pvTable, Statistics statistics)
        {
            _timeStrategy = timeStrategy;
            _statistics = statistics;
        }

        public Score Search(Position position, Score alpha, Score beta, int ply)
        {
            return InnerSearch(position, 1, alpha, beta, ply);
        }

        public Score InnerSearch(Position position, int depth, Score alpha, Score beta, int ply)
        {
            _statistics.QSearchNonRootNodes++;

            _statistics.MaxPly = Math.Max(_statistics.MaxPly, ply);
            _statistics.Evaluations++;
            var standPatEval = _evaluator.Evaluate(position);
            // correct so side to move wants to maximize value
            if (position.SideToMove == Color.Black)
                standPatEval = -standPatEval;

            if (standPatEval >= beta)
            {
                return standPatEval;
            }

            bool raisedAlpha = false;

            // TODO: what to do if standpat does/doesn't raise alpha???
            if (standPatEval > alpha)
            {
                raisedAlpha = true;
                alpha = standPatEval;
            }

            // for now, let's do this to prevent qsearch explosion
            // TODO: should we do something in order to return a conservatively low value?
            if (depth > MaxDepth)
                return standPatEval;

            var moves = _moveListCache.Get(ply);
            moves.Clear();

            bool moveGenIsStrictlyLegal;
            IEnumerable<Move> moveEnumerator;
            if (position.InCheck())
            {
                // if we're in check, we need to try all moves
                _moveGenerator.Generate(moves, position);
                moveGenIsStrictlyLegal = _moveGenerator.OnlyLegalMoves;
                moveEnumerator = _checkEvasionsMoveOrderer.Order(moves, position);
            }
            else
            {
                // if not in check, then only look at captures and promotions
                _moveGenerator.Generate(moves, position);
                moveGenIsStrictlyLegal = _moveGenerator.OnlyLegalMoves;

                // since we don't have a qsearch movegen, we need to remove the non-applicable moves
                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    var move = moves[i];
                    if (!move.MoveType.HasFlag(MoveType.Capture) && !move.MoveType.HasFlag(MoveType.Promotion))
                        moves.QuickRemoveAt(i);
                }

                // TODO: we should use SEE to determine which moves to keep!

                moveEnumerator = _quiescenceMoveOrderer.Order(moves, position);
            }

            int moveNumber = 0;
            var cachedPositionObject = _positionCache.Get(ply);
            foreach (var move in moveEnumerator)
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);
                
                if (!moveGenIsStrictlyLegal && nextPosition.MovedIntoCheck())
                    continue;

                moveNumber++;

                var eval = -InnerSearch(nextPosition, depth + 1, -beta, -alpha, ply + 1);

                if (eval >= beta)
                {
                    _statistics.QSearchCutNodes++;
                    _statistics.QSearchCutMoveMisses += moveNumber - 1; // don't include the current move in the move misses calculation
                    return eval;
                }

                if (eval > alpha)
                {
                    raisedAlpha = true;
                    alpha = eval;
                }
            }

            // TODO: if there were no legal moves, then maybe we should do a terminal check? it's dangerous not to, but maybe kinda slow to do this

            if (raisedAlpha)
            {
                // TODO: eventually commit to triangular pv table? Maybe?
                _statistics.QSearchPVNodes++;
            }
            else
            {
                _statistics.QSearchAllNodes++;
            }

            return alpha;
        }
    }
}
