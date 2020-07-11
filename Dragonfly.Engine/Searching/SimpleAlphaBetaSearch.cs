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
        private readonly IEvaluator _evaluator; // TODO: do we need this?
        private readonly IQSearch _qSearch;
        private ITimeStrategy _timeStrategy;
        private Statistics _statistics;
        private int _enteredCount; // this is used to only call _timeStrategy occasionally, instead on every entry of InnerSearch()
        private readonly ObjectCacheByDepth<Position> _positionCache = new ObjectCacheByDepth<Position>();
        private readonly ObjectCacheByDepth<List<Move>> _moveListCache = new ObjectCacheByDepth<List<Move>>();

        public SimpleAlphaBetaSearch(IMoveGen moveGen, IEvaluator evaluator, IQSearch qSearch)
        {
            _moveGen = moveGen;
            _evaluator = evaluator;
            _qSearch = qSearch;
        }

        public (Move move, Statistics statistics) Search(Position position, ITimeStrategy timeStrategy)
        {
            // This is non-reentrant, so let's make sure nobody accidentally calls us twice
            lock (this)
            {
                // setup
                _timeStrategy = timeStrategy;
                _enteredCount = 0;
                _statistics = new Statistics();
                _statistics.StartTime = DateTime.Now;
                _qSearch.StartSearch(_timeStrategy, _statistics);

                Move bestMove = Move.Null;
                var moveList = new List<Move>();
                _moveGen.Generate(moveList, position);

                // TODO: instead of looping here, why don't we only loop in InnerSearch and get the best value from the PV table?
                // That would simplify things a lot.
                // However, if we have aspiration windows and we get a beta cutoff, how do we retrieve the best move? Or is that even required?
                // The PV table would probably need to handle that case.
                var tmpBestMove = Move.Null;
                for (int depth = 1;; depth++)
                {
                    Score alpha = Score.MinValue;
                    Score beta = Score.MaxValue;

                    var cachedPositionObject = new Position();
                    foreach (var move in moveList)
                    {
                        var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                        if (!_moveGen.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                            continue;

                        _statistics.CurrentDepth = depth;
                        var nextEval = -InnerSearch(nextPosition, depth-1, -beta, -alpha, 1);

                        if (timeStrategy.ShouldStop(_statistics))
                            return (bestMove, _statistics);

                        if (nextEval > alpha)
                        {
                            alpha = nextEval;
                            tmpBestMove = move;
                        }
                    }

                    // only committing best move after a full search
                    // TODO: this will go away once we're no longer doing a search at this level
                    bestMove = tmpBestMove;
                }
            }
        }

        // TODO: name actualDepth and depth appropriately
        private Score InnerSearch(Position position, int depth, Score alpha, Score beta, int ply)
        {
            // do it every 64? hmm..
            if (_enteredCount % 64 == 0 && _timeStrategy.ShouldStop(_statistics))
                return 0; // dummy value won't be used

            // It's important we increment _after_ checking, because if we're stopping, we don't want entered count to be increasing
            _enteredCount++;

            // Note: we don't return draw on 50 move counter; if the engine doesn't know how to make progress in 50 moves, telling the engine it's about to draw can only induce mistakes.
            if (position.RepetitionNumber >= 3)
            {
                _statistics.TerminalNodes++;
                // TODO: contempt
                return 0; // draw
            }

            if (depth <= 0)
            {
                // Note: don't increase ply, as it's evaluating this position (same as current ply)
                // We don't need to count this, as qsearch does its own node counting
                return _qSearch.Search(position, alpha, beta, ply);
            }

            var moveList = _moveListCache.Get(ply);
            moveList.Clear();

            _moveGen.Generate(moveList, position);

            bool anyMoves = false;
            bool raisedAlpha = false;

            var cachedPositionObject = _positionCache.Get(ply);
            foreach (var move in moveList)
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                if (!_moveGen.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                    continue;

                anyMoves = true;

                var eval = -InnerSearch(nextPosition, depth - 1, -beta, -alpha, ply+1);
                if (eval >= beta)
                {
                    _statistics.InternalCutNodes++;
                    // TODO: move ordering stats

                    return eval; // fail soft, but shouldn't matter for this naive implementation
                }

                if (eval > alpha)
                {
                    raisedAlpha = true;
                    // TODO eventually: we need to store this into triangular pv table somehow
                    alpha = eval;
                }
            }

            if (!anyMoves)
            {
                _statistics.TerminalNodes++;

                if (position.InCheck())
                    return Score.GetMateScore(position.SideToMove, position.GamePly);
                else
                    return 0; // draw; TODO: contempt
            }

            if (raisedAlpha)
            {
                // TODO: eventually commit to triangular pv table
                _statistics.InternalPVNodes++;
            }
            else
            {
                _statistics.InternalAllNodes++;
            }

            return alpha;
        }
    }
}
