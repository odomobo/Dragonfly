using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    // TODO: name this something more useful
    public interface IProtocol
    {
        void PrintInfo(Statistics statistics);
        void PrintBestMove(Move bestMove);
        void PrintDebugMessage(string message);
    }
}
