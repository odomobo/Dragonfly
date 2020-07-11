using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class NodeCountStrategy : ITimeStrategy
    {
        private readonly int _nodeCount;
        private bool _stopping;

        public NodeCountStrategy(int nodeCount)
        {
            _nodeCount = nodeCount;
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

            if (statistics.Nodes >= _nodeCount)
            {
                _stopping = true;
                return true;
            }

            return false;
        }
    }
}
