using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class DefaultTimeStrategy : ITimeStrategy
    {
        private readonly SynchronizedFlag _stopping = new SynchronizedFlag();

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
            return _stopping.IsSet();
        }
    }
}
