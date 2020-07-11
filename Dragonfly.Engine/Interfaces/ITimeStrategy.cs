using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.Interfaces
{
    public interface ITimeStrategy
    {
        void Start();

        void ForceStop();

        bool ShouldStop(Statistics statistics);
    }
}
