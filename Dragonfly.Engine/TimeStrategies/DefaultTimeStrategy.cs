using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class DefaultTimeStrategy : ITimeStrategy
    {
        private bool _stopping;

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
            return _stopping;
        }
    }
}
