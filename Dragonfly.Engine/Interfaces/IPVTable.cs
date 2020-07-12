using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface IPVTable
    {
        /// <summary>
        /// You add a move before you start searching it.
        /// This is because we don't know whether or not this move might be a PV move until after it's too late.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="ply"></param>
        void Add(Move move, int ply);

        /// <summary>
        /// You commit a move once you've completed searching the move and it appears to be a PV move.
        /// This will overwrite any other move you might have committed at this depth.
        /// Committing a move at ply 0 makes it the "best line"
        /// </summary>
        /// <param name="ply"></param>
        void Commit(int ply);

        /// <summary>
        /// This should return a copy of the best line.
        /// </summary>
        /// <returns></returns>
        List<Move> GetBestLine();

        Move GetCurrentMove();
    }
}
