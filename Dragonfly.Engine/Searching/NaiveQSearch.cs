using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.Searching
{
    public class NaiveQSearch : IQSearch
    {
        private readonly IEvaluator _evaluator;
        private ITimeStrategy _timeStrategy;
        private Statistics _statistics;

        public NaiveQSearch(IEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public void StartSearch(ITimeStrategy timeStrategy, Statistics statistics)
        {
            _timeStrategy = timeStrategy;
            _statistics = statistics;
        }

        public Score Search(Position position, int ply)
        {
            _statistics.Evaluations++;
            _statistics.MaxPly = Math.Max(_statistics.MaxPly, ply);

            var eval = _evaluator.Evaluate(position);
            if (position.SideToMove == Color.White)
                return eval;
            else
                return -eval;
        }
    }
}
