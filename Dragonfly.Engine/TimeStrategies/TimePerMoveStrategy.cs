using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class TimePerMoveStrategy : ITimeStrategy
    {
        private readonly TimeSpan _timeSpan;
        private bool _stopping;

        public TimePerMoveStrategy(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public void Start()
        {
            _stopping = false;
        }

        public void ForceStop()
        {
            _stopping = true;
        }

        public bool ShouldStop(Statistics statistics)
        {
            if (_stopping)
                return true;

            if ((DateTime.Now - statistics.StartTime) >= _timeSpan)
            {
                _stopping = true;
                return true;
            }

            return false;
        }
    }
}
