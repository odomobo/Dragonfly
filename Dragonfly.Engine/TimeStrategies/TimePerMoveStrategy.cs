using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class TimePerMoveStrategy : ITimeStrategy
    {
        private readonly TimeSpan _timeSpan;
        private readonly SynchronizedFlag _stopping = new SynchronizedFlag();

        public TimePerMoveStrategy(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public void Start()
        {
            _stopping.Clear();
        }

        public void ForceStop()
        {
            _stopping.Set();
        }

        public bool ShouldStop(Statistics statistics)
        {
            if (_stopping.IsSet())
                return true;

            if ((DateTime.Now - statistics.StartTime) >= _timeSpan)
            {
                _stopping.Set();
                return true;
            }

            return false;
        }
    }
}
