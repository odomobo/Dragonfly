using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class DepthStrategy : ITimeStrategy
    {
        private readonly int _depth;
        private readonly SynchronizedFlag _stopping = new SynchronizedFlag();

        public DepthStrategy(int depth)
        {
            _depth = depth;
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

            // we basically start the next iteration, and then break out of it instantly; this is probably the simplest way to do it
            if (statistics.CurrentDepth > _depth)
            {
                _stopping.Set();
                return true;
            }

            return false;
        }
    }
}
