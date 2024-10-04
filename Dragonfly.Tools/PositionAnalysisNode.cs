using Dragonfly.Engine.CoreTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public class PositionAnalysisNode
    {
        public string Fen { get; set; }
        public ulong Hash { get; set; }
        public Dictionary<string, Score> MoveScores { get; set; }
    }
}
