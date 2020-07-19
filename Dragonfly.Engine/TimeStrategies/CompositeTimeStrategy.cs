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
        private readonly SynchronizedFlag _stopping = new SynchronizedFlag();

        public CompositeTimeStrategy(IEnumerable<ITimeStrategy> timeStrategies)
        {
            _timeStrategies = timeStrategies.ToList();
        }

        public void Start()
        {
            _stopping.Clear();
            foreach (var timeStrategy in _timeStrategies)
                timeStrategy.Start();
        }

        public void ForceStop()
        {
            _stopping.Set();
        }

        public bool ShouldStop(Statistics statistics)
        {
            if (_stopping.IsSet())
                return true;

            if (_timeStrategies.Any(ts => ts.ShouldStop(statistics)))
            {
                _stopping.Set();
                return true;
            }

            return false;
        }
    }
}
