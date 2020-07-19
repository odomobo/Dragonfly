using System;
using System.Collections;
using System.Text.RegularExpressions;
using Dragonfly.Engine.MoveGeneration;
using NUnit.Framework;

namespace Dragonfly.Engine.Tests
{
    [TestFixture]
    public class PerftTests
    {
        private MoveGenerator _moveGenerator;
        private Perft _perft;

        [SetUp]
        public void Setup()
        {
            _moveGenerator = new MoveGenerator();
            _perft = new Perft(_moveGenerator);
        }

        [TestCaseSource(typeof(PerftData), nameof(PerftData.PerftTestCases))]
        public int PerftTest(string fen, int depth)
        {
            var board = BoardParsing.PositionFromFen(fen);
            return _perft.GoPerft(board, depth);
        }
    }

    [TestFixture]
    public class PerftHashingTests
    {
        private MoveGenerator _moveGenerator;
        private Perft _perftWithHashing;

        [SetUp]
        public void Setup()
        {
            _moveGenerator = new MoveGenerator();
            _perftWithHashing = new Perft(_moveGenerator, 1_000_000);
        }

        [TestCaseSource(typeof(PerftData), nameof(PerftData.PerftTestCases))]
        public int PerftWithHashingTest(string fen, int depth)
        {
            var board = BoardParsing.PositionFromFen(fen);
            return _perftWithHashing.GoPerft(board, depth);
        }
    }

    public class PerftData
    {
        private static readonly Regex PerftResultRegex = new Regex(@"D(\d+) (\d+)");
        public static IEnumerable PerftTestCases
        {
            get
            {
                foreach (var line in Enumerators.GetPerftCasesEnumerator())
                {
                    var splitLine = line.Split(';', 2);

                    var fen = splitLine[0].Trim();
                    var perftCases = splitLine[1];

                    foreach (var perftCase in perftCases.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var match = PerftResultRegex.Match(perftCase);
                        int depth = int.Parse(match.Groups[1].Value);
                        int perftResult = int.Parse(match.Groups[2].Value);

                        yield return new TestCaseData(fen, depth).Returns(perftResult);
                    }
                }
            }
        }
    }
}