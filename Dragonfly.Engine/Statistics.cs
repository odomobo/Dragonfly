using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.PVTable;

namespace Dragonfly.Engine
{
    public sealed class Statistics
    {
        public delegate void PrintInfoDelegate(Statistics statistics);

        public DateTime StartTime;
        public Color SideCalculating;
        public List<Move> BestLine = new List<Move>();
        public Score CurrentScore;

        public int Nodes => InternalCutNodes + InternalPVNodes + InternalAllNodes +
                            QSearchCutNodes + QSearchPVNodes + QSearchAllNodes +
                            Evaluations;

        // If not differentiating between internal and qsearch nodes, should only use internal node counts.
        // If not differentiating between cut/pv/all nodes, should only use PV value.

        public int InternalCutNodes;
        public int InternalPVNodes;
        public int InternalAllNodes;
        public int InternalMovesEvaluated; // this helps us determine average branching factor
        public int InternalCutMoveMisses; // how many moves were evaluated before the cut move

        public double InternalBranchingFactor =>
            (double) InternalMovesEvaluated / (InternalCutNodes + InternalPVNodes + InternalAllNodes);

        public double InternalAverageCutMoveMisses => (double) InternalCutMoveMisses / InternalCutNodes;

        public int QSearchCutNodes;
        public int QSearchPVNodes;
        public int QSearchAllNodes;
        public int QSearchMovesEvaluated; // this helps us determine average branching factor
        public int QSearchCutMoveMisses; // how many moves were evaluated before the cut move

        public double QSearchBranchingFactor =>
            (double)QSearchMovesEvaluated / (QSearchCutNodes + QSearchPVNodes + QSearchAllNodes);

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
