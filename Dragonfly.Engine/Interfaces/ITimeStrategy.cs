using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.Interfaces
{
    interface ITimeStrategy
    {
        void Start();
        bool ShouldStop();
    }
}
