using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.MoveGeneration.Tables
{
    public static class KnightMoveTable
    {
        private static readonly ulong[] KnightMovesLookup = GenerateKnightMovesLookup();

        private static ulong[] GenerateKnightMovesLookup()
        {
            var kingMovesLookup = new ulong[64];
            foreach (var (srcFile, srcRank) in Position.GetAllFilesRanks())
            {
                ulong moves = 0;

                // tall L jumps first
                for (int dstRank = srcRank - 2; dstRank <= srcRank + 2; dstRank+=4)
                {
                    for (int dstFile = srcFile - 1; dstFile <= srcFile + 1; dstFile+=2)
                    {
                        // can't move off board
                        if (!Position.FileRankOnBoard(dstFile, dstRank))
                            continue;

                        var dstIx = Position.IxFromFileRank(dstFile, dstRank);
                        moves |= Position.ValueFromIx(dstIx);
                    }
                }

                // wide L jumps second
                for (int dstRank = srcRank - 1; dstRank <= srcRank + 1; dstRank += 2)
                {
                    for (int dstFile = srcFile - 2; dstFile <= srcFile + 2; dstFile += 4)
                    {
                        // can't move off board
                        if (!Position.FileRankOnBoard(dstFile, dstRank))
                            continue;

                        var dstIx = Position.IxFromFileRank(dstFile, dstRank);
                        moves |= Position.ValueFromIx(dstIx);
                    }
                }

                var ix = Position.IxFromFileRank(srcFile, srcRank);
                kingMovesLookup[ix] = moves;
            }

            return kingMovesLookup;
        }

        public static ulong GetMoves(int ix)
        {
            return KnightMovesLookup[ix];
        }
    }
}
