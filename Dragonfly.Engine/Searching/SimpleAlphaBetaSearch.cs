using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.PerformanceTypes;
using Dragonfly.Engine.PVTable;

namespace Dragonfly.Engine.Searching
{
    public sealed class SimpleAlphaBetaSearch : ISearch
    {
        private readonly IMoveGenerator _moveGenerator;
        private readonly IEvaluator _evaluator; // TODO: do we need this?
        private readonly IQSearch _qSearch;
        private IPVTable _pvTable;
        private ITimeStrategy _timeStrategy;
        private Statistics _statistics;
        private int _enteredCount; // this is used to only call _timeStrategy occasionally, instead on every entry of InnerSearch()
        private readonly ObjectCacheByDepth<Position> _positionCache = new ObjectCacheByDepth<Position>();
        private readonly ObjectCacheByDepth<List<Move>> _moveListCache = new ObjectCacheByDepth<List<Move>>();

        public SimpleAlphaBetaSearch(IMoveGenerator moveGenerator, IEvaluator evaluator, IQSearch qSearch)
        {
            _moveGenerator = moveGenerator;
            _evaluator = evaluator;
            _qSearch = qSearch;
        }

        public (Move move, Statistics statistics) Search(Position position, ITimeStrategy timeStrategy, Statistics.PrintInfoDelegate printInfoDelegate)
        {
            // setup
            _timeStrategy = timeStrategy;
            _enteredCount = 0;
            _statistics = new Statistics();
            _statistics.Timer.Start();
            _statistics.SideCalculating = position.SideToMove;
            _pvTable = new TriangularPVTable(); // TODO: should we be passing this in instead?
            _qSearch.StartSearch(_timeStrategy, _pvTable, _statistics);

            Move bestMove = Move.Null;
            var moveList = new List<Move>();
            _moveGenerator.Generate(moveList, position);

            // TODO: instead of looping here, why don't we only loop in InnerSearch and get the best value from the PV table?
            // That would simplify things a lot.
            // However, if we have aspiration windows and we get a beta cutoff, how do we retrieve the best move? Or is that even required?
            // The PV table would probably need to handle that case.
            var tmpBestMove = Move.Null;
            for (int depth = 1;; depth++)
            {
                _statistics.NormalNonLeafNodes++;

                Score alpha = Score.MinValue;
                Score beta = Score.MaxValue;

                var cachedPositionObject = new Position();
                foreach (var move in moveList)
                {
                    var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                    if (!_moveGenerator.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                        continue;

                    _pvTable.Add(move, 0);

                    _statistics.CurrentDepth = depth;
                    var nextEval = -InnerSearch(nextPosition, depth-1, -beta, -alpha, 1);

                    if (timeStrategy.ShouldStop(_statistics))
                    {
                        _statistics.Timer.Stop();
                        return (bestMove, _statistics);
                    }

                    if (nextEval > alpha)
                    {
                        alpha = nextEval;
                        tmpBestMove = move;
                        _pvTable.Commit(0);
                    }
                }

                // only committing best move after a full search
                // TODO: this will go away once we're no longer doing a search at this level
                bestMove = tmpBestMove;
                _statistics.BestLine = _pvTable.GetBestLine();
                if (position.SideToMove == Color.White)
                    _statistics.CurrentScore = alpha;
                else
                    _statistics.CurrentScore = -alpha;

                // if we don't do this, we'll never return from a terminal position (with no moves)
                if (timeStrategy.ShouldStop(_statistics))
                {
                    _statistics.Timer.Stop();
                    return (bestMove, _statistics);
                }

                // if we didn't return, let's print some info!
                printInfoDelegate(_statistics);
            }
        }

        private Score InnerSearch(Position position, int depth, Score alpha, Score beta, int ply)
        {
            // do it every 64? hmm..
            if (_enteredCount % 64 == 0 && _timeStrategy.ShouldStop(_statistics))
                return 0; // dummy value won't be used

            // It's important we increment _after_ checking, because if we're stopping, we don't want entered count to be increasing
            _enteredCount++;

            // by nature of this being called, we know this is a non-root node
            _statistics.NormalNonRootNodes++;

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

            _moveGenerator.Generate(moveList, position);

            bool anyMoves = false;
            bool raisedAlpha = false;

            int moveNumber = 0;
            var cachedPositionObject = _positionCache.Get(ply);
            foreach (var move in moveList)
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                if (!_moveGenerator.OnlyLegalMoves && nextPosition.MovedIntoCheck())
                    continue;

                moveNumber++;

                anyMoves = true;
                _pvTable.Add(move, ply);
                
                var eval = -InnerSearch(nextPosition, depth - 1, -beta, -alpha, ply+1);
                if (eval >= beta)
                {
                    _statistics.NormalNonLeafNodes++;
                    _statistics.NormalCutNodes++;
                    _statistics.NormalCutMoveMisses += moveNumber - 1; // don't include the current move in the move misses calculation

                    return eval; // fail soft, but shouldn't matter for this naive implementation
                }

                if (eval > alpha)
                {
                    raisedAlpha = true;
                    alpha = eval;
                    _pvTable.Commit(ply);
                }
            }

            if (!anyMoves)
            {
                _statistics.TerminalNodes++;

                if (position.InCheck())
                    return Score.GetMateScore(position.GamePly);
                else
                    return 0; // draw; TODO: contempt
            }

            _statistics.NormalNonLeafNodes++;
            if (raisedAlpha)
            {
                _statistics.NormalPVNodes++;
            }
            else
            {
                _statistics.NormalAllNodes++;
            }

            return alpha;
        }
    }
}
