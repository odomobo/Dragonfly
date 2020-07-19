using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.TimeStrategies
{
    public sealed class ClockTimeStrategy : ITimeStrategy
    {
        private readonly TimeSpan _ourTimeRemaining;
        private readonly TimeSpan? _ourIncrement;
        private readonly TimeSpan? _otherTimeRemaining;
        private readonly TimeSpan? _otherIncrement;
        private readonly int? _movesToNextTimeControl;
        private readonly SynchronizedFlag _stopping = new SynchronizedFlag();

        public ClockTimeStrategy(
            TimeSpan ourTimeRemaining,
            TimeSpan? ourIncrement,
            TimeSpan? otherTimeRemaining,
            TimeSpan? otherIncrement,
            int? movesToNextTimeControl
        )
        {
            _ourTimeRemaining = ourTimeRemaining;
            _ourIncrement = ourIncrement;
            _otherTimeRemaining = otherTimeRemaining;
            _otherIncrement = otherIncrement;
            _movesToNextTimeControl = movesToNextTimeControl;
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

            // TODO: make this more sophisticated:
            // - buffer time (useful for short time controls)
            // - use increment to use more time
            // - next time control, to spend more time if we have another time control
            // - pay attention to search panicking, to allow more time when panicking
            var timeToSpend = _ourTimeRemaining / 30;
            var timeElapsed = statistics.Timer.Elapsed;
            if (timeElapsed > timeToSpend)
            {
                _stopping.Set();
                return true;
            }

            return false;
        }
    }
}
