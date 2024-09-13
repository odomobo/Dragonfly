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
        private readonly IGameTerminator _gameTerminator;
        private readonly IEvaluator _evaluator; // TODO: do we need this?
        private readonly IQSearch _qSearch;
        private readonly CompositeMoveOrderer _moveOrderer;
        private IPVTable _pvTable;
        private ITimeStrategy _timeStrategy;
        private Statistics _statistics;
        private int _enteredCount; // this is used to only call _timeStrategy occasionally, instead on every entry of InnerSearch()

        public IidAlphaBetaSearch(IMoveGenerator moveGenerator, IGameTerminator gameTerminator, IEvaluator evaluator, IQSearch qSearch, CompositeMoveOrderer moveOrderer)
        {
            _moveGenerator = moveGenerator;
            _gameTerminator = gameTerminator;
            _evaluator = evaluator;
            _qSearch = qSearch;
            _moveOrderer = moveOrderer;
        }

        public (Move move, Statistics statistics) Search(Position position, ITimeStrategy timeStrategy, IProtocol protocol)
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
            var moveList = new StaticList256<Move>();
            _moveGenerator.Generate(ref moveList, position);

            if (!_moveGenerator.OnlyLegalMoves)
            {
                // remove all illegal moves
                for (int i = moveList.Count - 1; i >= 0; i--)
                {
                    var move = moveList[i];
                    var testingPosition = position.MakeMove(move);
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
                _statistics.NormalNonLeafNodes++;

                Score alpha = Score.MinValue;
                Score beta = Score.MaxValue;

                for (int i = 0; i < scoredMoveList.Count; i++)
                {
                    var move = scoredMoveList[i].Move;

                    var nextPosition = position.MakeMove(move);
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

                        protocol.PrintInfo(_statistics);
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
        private void SortWithIid(Position position, ref StaticList256<Move> moves, int depth, Score alpha, Score beta, int ply)
        {
            // SortWithIid is considered a root node; it's not a leaf of the parent caller, it's more of a helper
            _statistics.NormalNonLeafNodes++;

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
            var scoredMoves = new StaticList256<ScoredMove>();

            _moveOrderer.Sort(ref moves, position);
            foreach (var move in moves)
            {
                var nextPosition = position.MakeMove(move);

                var score = -InnerSearch(nextPosition, (depth/2) - 1, true, -beta, -alpha, ply+1);
                // leave a buffer for better ordering
                if (score-50 > alpha)
                    alpha = score-50;
                //else if (score == alpha)
                //    score -= 1; // artificially lower alpha cutoffs so they don't look like PVs

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

            // by nature of this being called, we know this is a non-root node
            _statistics.NormalNonRootNodes++;

            if (_gameTerminator.IsPositionTerminal(position, out var terminalScore))
            {
                _statistics.TerminalNodes++;
                return terminalScore;
            }

            if (depth <= 0)
            {
                // Note: don't increase ply, as it's evaluating this position (same as current ply)
                // We don't need to count this, as qsearch does its own node counting
                return _qSearch.Search(position, alpha, beta, ply);
            }

            var moveList = new StaticList256<Move>();

            _moveGenerator.Generate(ref moveList, position);

            if (!_moveGenerator.OnlyLegalMoves)
            {
                // remove all illegal moves
                for (int i = moveList.Count - 1; i >= 0; i--)
                {
                    var move = moveList[i];
                    var testingPosition = position.MakeMove(move);
                    if (testingPosition.MovedIntoCheck())
                        moveList.QuickRemoveAt(i);
                }
            }

            if (!moveList.Any())
            {
                _statistics.TerminalNodes++;
                return _gameTerminator.NoLegalMovesScore(position);
            }

            // if depth is 1 or 2, then we'd literally just be searching twice
            if (depth > 1)
                SortWithIid(position, ref moveList, depth, alpha, beta, ply);

            bool raisedAlpha = false;
            int moveNumber = 0;
            foreach (var move in moveList)
            {
                var nextPosition = position.MakeMove(move);

                moveNumber++;

                if (!isIid)
                    _pvTable.Add(move, ply);

                bool lateMove = moveNumber > 5;

                int reduction = lateMove ? 1 : 0; // TODO: only do late move reduction for moves with improved ordering?
                var eval = -InnerSearch(nextPosition, depth - 1 - reduction, isIid, -beta, -alpha, ply+1);
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
                    if (!isIid)
                        _pvTable.Commit(ply);
                }
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
