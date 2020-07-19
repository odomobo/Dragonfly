using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveOrdering;
using Dragonfly.Engine.PerformanceTypes;
using Dragonfly.Engine.PVTable;

namespace Dragonfly.Engine.Searching
{
    public sealed class IidAlphaBetaSearch : ISearch
    {
        private readonly struct ScoredMove
        {
            public readonly Move Move;
            public readonly Score Score;

            public ScoredMove(Move move, Score score)
            {
                Move = move;
                Score = score;
            }
        }

        private readonly IMoveGenerator _moveGenerator;
        private readonly IEvaluator _evaluator; // TODO: do we need this?
        private readonly IQSearch _qSearch;
        private readonly CompositeMoveOrderer _moveOrderer;
        private IPVTable _pvTable;
        private ITimeStrategy _timeStrategy;
        private Statistics _statistics;
        private int _enteredCount; // this is used to only call _timeStrategy occasionally, instead on every entry of InnerSearch()
        private readonly ObjectCacheByDepth<Position> _positionCache = new ObjectCacheByDepth<Position>();
        private readonly ObjectCacheByDepth<List<Move>> _moveListCache = new ObjectCacheByDepth<List<Move>>();
        private readonly ObjectCacheByDepth<List<ScoredMove>> _iidScoredMoveListCache = new ObjectCacheByDepth<List<ScoredMove>>();

        public IidAlphaBetaSearch(IMoveGenerator moveGenerator, IEvaluator evaluator, IQSearch qSearch, CompositeMoveOrderer moveOrderer)
        {
            _moveGenerator = moveGenerator;
            _evaluator = evaluator;
            _qSearch = qSearch;
            _moveOrderer = moveOrderer;
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

            var cachedPositionObject = new Position();

            if (!_moveGenerator.OnlyLegalMoves)
            {
                // remove all illegal moves
                for (int i = moveList.Count - 1; i >= 0; i--)
                {
                    var move = moveList[i];
                    var testingPosition = Position.MakeMove(cachedPositionObject, move, position);
                    if (testingPosition.MovedIntoCheck())
                        moveList.QuickRemoveAt(i);
                }
            }

            var scoredMoveList = new List<ScoredMove>();
            foreach (var move in moveList)
            {
                scoredMoveList.Add(new ScoredMove(move, 0));
            }

            // TODO: instead of looping here, why don't we only loop in InnerSearch and get the best value from the PV table?
            // That would simplify things a lot.
            // However, if we have aspiration windows and we get a beta cutoff, how do we retrieve the best move? Or is that even required?
            // The PV table would probably need to handle that case.
            for (int depth = 1;; depth++)
            {
                Score alpha = Score.MinValue;
                Score beta = Score.MaxValue;

                for (int i = 0; i < scoredMoveList.Count; i++)
                {
                    var move = scoredMoveList[i].Move;

                    var nextPosition = Position.MakeMove(cachedPositionObject, move, position);
                    _pvTable.Add(move, 0);

                    _statistics.CurrentDepth = depth;
                    var nextEval = -InnerSearch(nextPosition, depth-1, false, -beta, -alpha, 1);
                    if (nextEval == alpha)
                        nextEval -= 1;

                    scoredMoveList[i] = new ScoredMove(move, nextEval);

                    if (timeStrategy.ShouldStop(_statistics))
                    {
                        _statistics.Timer.Stop();
                        return (bestMove, _statistics);
                    }

                    if (nextEval > alpha)
                    {
                        alpha = nextEval;
                        bestMove = move; // this is safe, because we search the best move from last pass first in the next pass
                        _pvTable.Commit(0);
                        _statistics.BestLine = _pvTable.GetBestLine();
                        if (position.SideToMove == Color.White)
                            _statistics.CurrentScore = alpha;
                        else
                            _statistics.CurrentScore = -alpha;

                        printInfoDelegate(_statistics);
                    }
                }

                // if we don't do this, we'll never return from a terminal position (with no moves)
                if (timeStrategy.ShouldStop(_statistics))
                {
                    _statistics.Timer.Stop();
                    return (bestMove, _statistics);
                }

                // sort the moves for next pass
                scoredMoveList.Sort((m1, m2) => (int)(m2.Score - m1.Score));
            }
        }

        // all moves need to be legal
        private void SortWithIid(Position position, List<Move> moves, int depth, Score alpha, Score beta, int ply)
        {
            var cachedPositionObject = _positionCache.Get(ply);
            // create .5 pawn buffer in alpha and beta
            alpha -= 50;
            if (alpha < Score.MinValue)
                alpha = Score.MinValue;

            beta += 50;
            if (beta > Score.MaxValue)
                beta = Score.MaxValue;

            //alpha = Score.MinValue;
            //beta = Score.MaxValue;

            // TODO: use a Span<ScoredMove>, once the version of .net core we're using supports Span.Sort()
            var scoredMoves = _iidScoredMoveListCache.Get(ply);
            scoredMoves.Clear();

            foreach (var move in _moveOrderer.Order(moves, position))
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                var score = -InnerSearch(nextPosition, depth - 2, true, -beta, -alpha, ply+1);
                // TODO: leave a buffer for better ordering?
                if (score > alpha)
                    alpha = score;
                else if (score == alpha)
                    score -= 1; // artificially lower alpha cutoffs so they don't look like PVs

                scoredMoves.Add(new ScoredMove(move, score));
            }

            // sort descending
            scoredMoves.Sort((m1, m2) => (int)(m2.Score - m1.Score));

            // copy back to moves
            for (int i = 0; i < scoredMoves.Count; i++)
            {
                moves[i] = scoredMoves[i].Move;
            }
        }

        private Score InnerSearch(Position position, int depth, bool isIid, Score alpha, Score beta, int ply)
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

            _moveGenerator.Generate(moveList, position);
            var cachedPositionObject = _positionCache.Get(ply);

            if (!_moveGenerator.OnlyLegalMoves)
            {
                // remove all illegal moves
                for (int i = moveList.Count - 1; i >= 0; i--)
                {
                    var move = moveList[i];
                    var testingPosition = Position.MakeMove(cachedPositionObject, move, position);
                    if (testingPosition.MovedIntoCheck())
                        moveList.QuickRemoveAt(i);
                }
            }

            if (!moveList.Any())
            {
                _statistics.TerminalNodes++;

                if (position.InCheck())
                    return Score.GetMateScore(position.GamePly);
                else
                    return 0; // draw; TODO: contempt
            }

            // if depth is 1 or 2, then we'd literally just be searching twice
            if (depth > 1)
                SortWithIid(position, moveList, depth, alpha, beta, ply);

            bool raisedAlpha = false;
            int moveNumber = 0;
            foreach (var move in moveList)
            {
                var nextPosition = Position.MakeMove(cachedPositionObject, move, position);

                moveNumber++;

                if (!isIid)
                    _pvTable.Add(move, ply);

                bool lateMove = moveNumber > 50; // TODO: disabled; enable this

                _statistics.InternalMovesEvaluated++;

                int reduction = lateMove && (depth > 1) ? 1 : 0;
                var eval = -InnerSearch(nextPosition, depth - 1 - reduction, isIid, -beta, -alpha, ply+1);
                if (eval >= beta)
                {
                    _statistics.InternalCutNodes++;
                    _statistics.InternalCutMoveMisses += moveNumber - 1; // don't include the current move in the move misses calculation

                    return eval; // fail soft, but shouldn't matter for this naive implementation
                }

                if (eval > alpha)
                {
                    raisedAlpha = true;
                    alpha = eval;
                    if (!isIid)
                        _pvTable.Commit(ply);
                }
            }

            if (raisedAlpha)
            {
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
