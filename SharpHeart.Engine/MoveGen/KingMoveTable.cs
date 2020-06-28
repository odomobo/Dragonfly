using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    internal static class KingMoveTable
    {
        private static readonly ulong[] KingMovesLookup = GenerateKingMovesLookup();

        private static ulong[] GenerateKingMovesLookup()
        {
            var kingMovesLookup = new ulong[64];
            foreach (var (srcFile, srcRank) in Board.GetAllFilesRanks())
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
                        if (!Board.FileRankOnBoard(dstFile, dstRank))
                            continue;

                        var dstIx = Board.IxFromFileRank(dstFile, dstRank);
                        srcBb |= Board.ValueFromIx(dstIx);
                    }
                }

                var ix = Board.IxFromFileRank(srcFile, srcRank);
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
