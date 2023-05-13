using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine
{
    public class DummyProtocol : IProtocol
    {
        public void PrintInfo(Statistics statistics)
        { }

        public void PrintBestMove(Move bestMove)
        { }

        public void PrintDebugMessage(string message)
        { }
    }
}
