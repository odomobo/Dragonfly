using Dragonfly.Engine;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public class GetBestMoveProtocol : IProtocol
    {
        public Move? BestMove { get; set; }
        public void PrintBestMove(Move bestMove)
        {
            BestMove = bestMove;
        }

        public void PrintDebugMessage(string message)
        {
        }

        public void PrintInfo(Statistics statistics)
        {
        }
    }
}
