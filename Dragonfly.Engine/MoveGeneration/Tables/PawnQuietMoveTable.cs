using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.MoveGeneration.Tables
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
            foreach (var (srcFile, srcRank) in Position.GetAllFilesRanks())
            {
                ulong singularCaptures = 0;
                int dstRank = srcRank + direction;
                if (Position.FileRankOnBoard(srcFile, dstRank))
                    singularCaptures |= Position.ValueFromFileRank(srcFile, dstRank);

                captures[Position.IxFromFileRank(srcFile, srcRank)] = singularCaptures;
            }

            return captures;
        }
    }
}
