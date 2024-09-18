using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.PerformanceTypes;

namespace Dragonfly.Engine.MoveOrdering
{
    public sealed class MvvLvaOrderer : IMoveOrderer
    {
        private readonly struct MvvLvaComparer : IComparer<Move>
        {
            private readonly Position _position;

            public MvvLvaComparer(Position position)
            {
                _position = position;
            }

            // we know these moves are captures
            public int Compare(Move x, Move y)
            {
                var xCapturedPieceType = _position.GetPiece(x.DstIx).PieceType;
                var yCapturedPieceType = _position.GetPiece(y.DstIx).PieceType;

                // We want a negative value (x < y) when x captured piece is stronger.
                // Larger piece int value means stronger piece.
                var capturedPieceComparison = (int)yCapturedPieceType - (int)xCapturedPieceType;

                if (capturedPieceComparison != 0)
                    return capturedPieceComparison;

                var xAttackingPieceType = _position.GetPiece(x.SourceIx).PieceType;
                var yAttackingPieceType = _position.GetPiece(y.SourceIx).PieceType;

                // We want a negative value (x < y) when x attacking piece is weaker.
                // Larger piece int value means stronger piece.
                var attackingPieceComparison = (int)xAttackingPieceType - (int)yAttackingPieceType;
                return attackingPieceComparison;
            }
        }

        public int PartitionAndSort(List<Move> moves, int start, int count, Position position)
        {
            int newCount = moves.PartitionBy(start, count, move => move.MoveType.HasFlag(MoveType.Capture));

            var comparer = new MvvLvaComparer(position);
            moves.Sort(start, newCount, comparer);

            return start + newCount;
        }
    }
}
