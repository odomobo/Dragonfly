using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.Interfaces
{
    interface ITimeStrategy
    {
        void Start();
        bool ShouldStop();
    }
}
