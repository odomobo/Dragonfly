using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.Searching
{
    // This doesn't actually perform qsearch; it shouldn't be used, as it will make very stupid decisions
    public class NaiveQSearch : IQSearch
    {
        private readonly IEvaluator _evaluator;
        private ITimeStrategy _timeStrategy;
        private Statistics _statistics;

        public NaiveQSearch(IEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public void StartSearch(ITimeStrategy timeStrategy, IPVTable pvTable, Statistics statistics)
        {
            _timeStrategy = timeStrategy;
            _statistics = statistics;
        }

        public Score Search(Position position, Score alpha, Score beta, int ply)
        {
            _statistics.MaxPly = Math.Max(_statistics.MaxPly, ply);
            _statistics.Evaluations++;
            
            var eval = _evaluator.Evaluate(position);
            if (position.SideToMove == Color.White)
                return eval;
            else
                return -eval;
        }
    }
}
