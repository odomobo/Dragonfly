using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpHeart.Engine.MoveGens;

namespace SharpHeart.Engine.Tests
{
    public class PerftTests
    {
        private MoveGen _moveGen;
        private Perft _perft;

        [SetUp]
        public void Setup()
        {
            _moveGen = new MoveGen();
            _perft = new Perft(_moveGen);
        }

        [TestCaseSource(typeof(PerftData), nameof(PerftData.PerftTestCases))]
        public int PerftTest(string fen, int depth)
        {
            var board = BoardParsing.BoardFromFen(fen);
            return _perft.GoPerft(board, depth);
        }
    }

    public class PerftData
    {
        private static readonly Regex PerftResultRegex = new Regex(@"D(\d+) (\d+)");
        public static IEnumerable PerftTestCases
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "SharpHeart.Engine.Tests.perfts.epd";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            break;

                        var splitLine = line.Split(';', 2);

                        var fen = splitLine[0];
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
}