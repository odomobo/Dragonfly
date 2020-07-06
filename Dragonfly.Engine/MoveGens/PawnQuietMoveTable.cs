using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.MoveGens
{
    public static class PawnQuietMoveTable
    {
        private static readonly ulong[][] QuietMoves = GenerateQuietMoves();

        private static ulong[][] GenerateQuietMoves()
        {
            var ret = new ulong[2][];

            foreach (var color in new[] { Color.White, Color.Black })
            {
                ret[(int)color] = GenerateQuietMoves(color);
            }

            return ret;
        }

        public static ulong GetMoves(int ix, Color color)
        {
            return QuietMoves[(int)color][ix];
        }

        private static ulong[] GenerateQuietMoves(Color color)
        {
            ulong[] captures = new ulong[64];
            int direction = color.GetPawnDirection();
            foreach (var (srcFile, srcRank) in Board.GetAllFilesRanks())
            {
                ulong singularCaptures = 0;
                int dstRank = srcRank + direction;
                if (Board.FileRankOnBoard(srcFile, dstRank))
                    singularCaptures |= Board.ValueFromFileRank(srcFile, dstRank);

                captures[Board.IxFromFileRank(srcFile, srcRank)] = singularCaptures;
            }

            return captures;
        }
    }
}
