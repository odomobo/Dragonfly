using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class CompositeTimeStrategy : ITimeStrategy
    {
        private readonly List<ITimeStrategy> _timeStrategies;
        private bool _stopping;

        public CompositeTimeStrategy(IEnumerable<ITimeStrategy> timeStrategies)
        {
            _timeStrategies = timeStrategies.ToList();
        }

        public void Start()
        {
            _stopping = false;
            foreach (var timeStrategy in _timeStrategies)
                timeStrategy.Start();
        }

        public void ForceStop()
        {
            _stopping = true;
        }

        public bool ShouldStop(Statistics statistics)
        {
            if (_stopping)
                return true;

            foreach (var timeStrategy in _timeStrategies)
            {
                if (timeStrategy.ShouldStop(statistics))
                    return true;
            }

            return false;
        }
    }
}
