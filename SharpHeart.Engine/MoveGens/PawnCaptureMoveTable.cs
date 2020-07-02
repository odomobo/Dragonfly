using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine.MoveGens
{
    public static class PawnCaptureMoveTable
    {
        private static readonly ulong[][] CaptureMoves = GenerateCaptureMoves();

        private static ulong[][] GenerateCaptureMoves()
        {
            var ret = new ulong[2][];

            foreach (var color in new[] { Color.White, Color.Black })
            {
                ret[(int)color] = GenerateCaptureMoves(color);
            }

            return ret;
        }

        public static ulong GetMoves(int ix, Color color)
        {
            return CaptureMoves[(int)color][ix];
        }

        private static ulong[] GenerateCaptureMoves(Color color)
        {
            ulong[] captures = new ulong[64];
            int direction = color.GetPawnDirection();
            foreach (var (srcFile, srcRank) in Board.GetAllFilesRanks())
            {
                ulong singularCaptures = 0;
                int dstRank = srcRank + direction;
                for (int dstFile = srcFile - 1; dstFile <= srcFile + 1; dstFile += 2)
                {
                    if (!Board.FileRankOnBoard(dstFile, dstRank))
                        continue;

                    singularCaptures |= Board.ValueFromFileRank(dstFile, dstRank);
                }

                captures[Board.IxFromFileRank(srcFile, srcRank)] = singularCaptures;
            }

            return captures;
        }
    }
}
