using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;

namespace Dragonfly.Engine.MoveOrdering
{
    public sealed class PromotionsOrderer : IMoveOrderer
    {
        private class PromotionsComparer : IComparer<Move>
        {
            public int Compare(Move x, Move y)
            {
                // We want a negative value (x < y) when x promoted piece is stronger.
                // Larger piece int value means stronger piece.
                var promotedPieceComparison = (int)y.PromotionPiece - (int)x.PromotionPiece;
                return promotedPieceComparison;
            }
        }

        private static readonly PromotionsComparer PromotionsComparerInstance = new PromotionsComparer();

        public int PartitionAndSort(List<Move> moves, int start, int count, Position position)
        {
            var newCount = moves.PartitionBy(start, count, move => move.MoveType.HasFlag(MoveType.Promotion));

            moves.Sort(start, newCount, PromotionsComparerInstance);

            return start + newCount;
        }
    }
}
