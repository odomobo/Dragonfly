using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.PVTable;

namespace Dragonfly.Engine
{
    public sealed class Statistics
    {
        public Stopwatch Timer = new Stopwatch();
        public Color SideCalculating;
        public List<Move> BestLine = new List<Move>();
        public Score CurrentScore;

        public int Nodes => NormalCutNodes + NormalPVNodes + NormalAllNodes +
                            QSearchCutNodes + QSearchPVNodes + QSearchAllNodes +
                            Evaluations;

        // If not differentiating between internal and qsearch nodes, should only use internal node counts.
        // If not differentiating between cut/pv/all nodes, should only use PV value.

        public int NormalCutNodes;
        public int NormalPVNodes;
        public int NormalAllNodes;
        public int NormalCutMoveMisses; // how many moves were evaluated before the cut move

        // Branching factor is the number of direct child nodes traversed for a particular node.
        // Average branching factor is the the mean branching factor, given all evaluated non-leaf positions.
        // Or: sum(branching factor for position i) / all positions i
        // Simplified, it becomes: sum(non-root positions) / sum(non-leaf positions)
        public int NormalNonRootNodes;
        public int NormalNonLeafNodes;
        public double NormalBranchingFactor =>
            (double) NormalNonRootNodes / NormalNonLeafNodes;

        public double NormalAverageCutMoveMisses => (double) NormalCutMoveMisses / NormalCutNodes;

        public int QSearchCutNodes;
        public int QSearchPVNodes;
        public int QSearchAllNodes;
        public int QSearchCutMoveMisses; // how many moves were evaluated before the cut move

        public int QSearchNonRootNodes;
        public int QSearchNonLeafNodes;
        public double QSearchBranchingFactor =>
            (double) QSearchNonRootNodes / QSearchNonLeafNodes;

        public double QSearchAverageCutMoveMisses => (double) QSearchCutMoveMisses / QSearchCutNodes;

        // Terminal nodes represent a game-over state.
        public int TerminalNodes;

        public int Evaluations;

        public int CurrentDepth;
        // TODO: should this only be set in qsearch?
        public int MaxPly;

        public bool Panicking; // we'll use this later

        // TODO: add move ordering stats
    }
}
