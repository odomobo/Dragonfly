using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.PerformanceTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface IMoveOrderer
    {
        /// <summary>
        /// Partitions the moves into relevant moves, and then sorts them in decreasing order of importance (most important moves first).
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="position"></param>
        /// <returns>The new start of the array</returns>
        public int PartitionAndSort(List<Move> moves, int start, int count, Position position);
    }
}
