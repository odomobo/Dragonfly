using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class NodeCountStrategy : ITimeStrategy
    {
        private readonly int _nodeCount;
        private readonly SynchronizedFlag _stopping = new SynchronizedFlag();

        public NodeCountStrategy(int nodeCount)
        {
            _nodeCount = nodeCount;
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

            if (statistics.Nodes >= _nodeCount)
            {
                _stopping.Set();
                return true;
            }

            return false;
        }
    }
}
