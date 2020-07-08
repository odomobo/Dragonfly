using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.MoveGeneration.Tables
{
    public static class KingMoveTable
    {
        private static readonly ulong[] KingMovesLookup = GenerateKingMovesLookup();

        private static ulong[] GenerateKingMovesLookup()
        {
            var kingMovesLookup = new ulong[64];
            foreach (var (srcFile, srcRank) in Position.GetAllFilesRanks())
            {
                ulong srcBb = 0;
                for (int dstRank = srcRank - 1; dstRank <= srcRank + 1; dstRank++)
                {
                    for (int dstFile = srcFile - 1; dstFile <= srcFile + 1; dstFile++)
                    {
                        // can't move to source square
                        if (dstRank == srcRank && dstFile == srcFile)
                            continue;

                        // can't move off board
                        if (!Position.FileRankOnBoard(dstFile, dstRank))
                            continue;

                        var dstIx = Position.IxFromFileRank(dstFile, dstRank);
                        srcBb |= Position.ValueFromIx(dstIx);
                    }
                }

                var ix = Position.IxFromFileRank(srcFile, srcRank);
                kingMovesLookup[ix] = srcBb;
            }

            return kingMovesLookup;
        }

        public static ulong GetMoves(int ix)
        {
            return KingMovesLookup[ix];
        }
    }
}
