using Dragonfly.Engine;
using Dragonfly.Engine.MoveGeneration;

namespace Dragonfly.Tools
{
    public class PgnToFen
    {
        public static void TransformToFenWithoutDuplicatePositions(string pgnFile, string fenFile)
        {
            using var pgnReader = new StreamReader(pgnFile);
            using var fenWriter = new StreamWriter(fenFile);

            var positionsSeenSoFar = new HashSet<ulong>();

            var moveGenerator = new MoveGenerator();
            var pgns = Pgn.ParsePgnStream(moveGenerator, pgnReader);
            foreach (var pgn in pgns)
            {
                foreach (var position in pgn.Positions)
                {
                    if (positionsSeenSoFar.Contains(position.ZobristHash))
                        continue;

                    fenWriter.WriteLine(BoardParsing.FenStringFromBoard(position));
                    positionsSeenSoFar.Add(position.ZobristHash);
                }
            }
        }
    }
}
