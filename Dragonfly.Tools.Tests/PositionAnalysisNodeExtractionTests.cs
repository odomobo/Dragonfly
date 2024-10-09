using Dragonfly.Engine.CoreTypes;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools.Tests
{
    [TestFixture]
    internal class PositionAnalysisNodeExtractionTests
    {
        [Test]
        [TestCaseSource(nameof(ExtractRelativeScoreTestCases))]
        public Score TestExtractRelativeScore(string line, int gamePly)
        {
            return PositionAnalysisNodeExtraction.ExtractRelativeScore(line, gamePly);
        }

        public static IEnumerable ExtractRelativeScoreTestCases
        {
            get
            {
                yield return new TestCaseData(
                    "info depth 2 seldepth 3 multipv 24 score cp -644 nodes 1406 nps 468666 hashfull 0 tbhits 0 time 3 pv e4f5 f7f5",
                    30)
                    .Returns(new Score(-644));
                yield return new TestCaseData(
                    "info depth 25 seldepth 50 multipv 3 score cp 317 nodes 15966870 nps 787088 hashfull 998 tbhits 0 time 20286 pv g2g4 f6f5 d3f5 a3c4 f1e2 f7f6 e2d3 c4d6 d3c3 e7d8 f5d3 d8c7 b4b5 d6c8 d3f5 c8b6 c3d3 b6d5 f5e4 d5f4 d3c4 f4h3 f2f3 h3f4 c4c5 f4e6 c5d5 e6f4 d5c4 c7b6 e4f5 f4h3 c4d5 b6b5 f5d3 b5b6",
                    80)
                    .Returns(new Score(317));
                yield return new TestCaseData("info depth 1 seldepth 2 multipv 13 score cp 36 nodes 263 nps 131500 hashfull 0 tbhits 0 time 2 pv d3h7",
                    80)
                    .Returns(new Score(36));
                yield return new TestCaseData("info depth 52 seldepth 10 multipv 1 score mate 5 nodes 16000315 nps 3213559 hashfull 192 tbhits 0 time 4979 pv g3a3 c2b1 e4e3 b1c2 a3b4 c2c1 e3d3 c1d1 b4d2",
                    114)
                    .Returns(Score.GetMateScore(Color.Black, 114 + 5));
                yield return new TestCaseData("info depth 245 seldepth 11 multipv 1 score mate -5 nodes 12403870 nps 4963533 hashfull 32 tbhits 0 time 2499 pv d2c2 e4d4 c2b2 d4c4 b2a2 g3g2 a2a1 c4c3 a1b1 g2b2",
                    113)
                    .Returns(Score.GetMateScore(Color.White, 113 + 5));
            }
        }

        [Test]
        [TestCaseSource(nameof(IsSearchInfoTestCases))]
        public bool TestIsSearchInfo(string line)
        {
            return PositionAnalysisNodeExtraction.IsSearchInfo(line);
        }

        public static IEnumerable IsSearchInfoTestCases
        {
            get
            {
                yield return new TestCaseData("info depth 2 seldepth 3 multipv 24 score cp -644 nodes 1406 nps 468666 hashfull 0 tbhits 0 time 3 pv e4f5 f7f5")
                    .Returns(true);
                yield return new TestCaseData("info depth 25 seldepth 50 multipv 3 score cp 317 nodes 15966870 nps 787088 hashfull 998 tbhits 0 time 20286 pv g2g4 f6f5 d3f5 a3c4 f1e2 f7f6 e2d3 c4d6 d3c3 e7d8 f5d3 d8c7 b4b5 d6c8 d3f5 c8b6 c3d3 b6d5 f5e4 d5f4 d3c4 f4h3 f2f3 h3f4 c4c5 f4e6 c5d5 e6f4 d5c4 c7b6 e4f5 f4h3 c4d5 b6b5 f5d3 b5b6")
                    .Returns(true);
                yield return new TestCaseData("info depth 52 seldepth 10 multipv 1 score mate 5 nodes 16000315 nps 3213559 hashfull 192 tbhits 0 time 4979 pv g3a3 c2b1 e4e3 b1c2 a3b4 c2c1 e3d3 c1d1 b4d2")
                    .Returns(true);
                yield return new TestCaseData("info depth 245 seldepth 11 multipv 1 score mate -5 nodes 12403870 nps 4963533 hashfull 32 tbhits 0 time 2499 pv d2c2 e4d4 c2b2 d4c4 b2a2 g3g2 a2a1 c4c3 a1b1 g2b2")
                    .Returns(true);
                yield return new TestCaseData("")
                    .Returns(false);
                yield return new TestCaseData("info string Available processors: 0-11")
                    .Returns(false);
                yield return new TestCaseData("info string NNUE evaluation using nn-1111cefa1111.nnue (133MiB, (22528, 3072, 15, 32, 1))")
                    .Returns(false);
                yield return new TestCaseData("info depth 222 currmove d2d1 currmovenumber 3")
                    .Returns(false);
                yield return new TestCaseData("bestmove d2c2 ponder e4d4")
                    .Returns(false);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetDepthCases))]
        public int TestGetDepth(string line)
        {
            return PositionAnalysisNodeExtraction.GetDepth(line);
        }

        public static IEnumerable GetDepthCases
        {
            get
            {
                yield return new TestCaseData("info depth 2 seldepth 3 multipv 24 score cp -644 nodes 1406 nps 468666 hashfull 0 tbhits 0 time 3 pv e4f5 f7f5")
                    .Returns(2);
                yield return new TestCaseData("info depth 25 seldepth 50 multipv 3 score cp 317 nodes 15966870 nps 787088 hashfull 998 tbhits 0 time 20286 pv g2g4 f6f5 d3f5 a3c4 f1e2 f7f6 e2d3 c4d6 d3c3 e7d8 f5d3 d8c7 b4b5 d6c8 d3f5 c8b6 c3d3 b6d5 f5e4 d5f4 d3c4 f4h3 f2f3 h3f4 c4c5 f4e6 c5d5 e6f4 d5c4 c7b6 e4f5 f4h3 c4d5 b6b5 f5d3 b5b6")
                    .Returns(25);
                yield return new TestCaseData("info depth 52 seldepth 10 multipv 1 score mate 5 nodes 16000315 nps 3213559 hashfull 192 tbhits 0 time 4979 pv g3a3 c2b1 e4e3 b1c2 a3b4 c2c1 e3d3 c1d1 b4d2")
                    .Returns(52);
                yield return new TestCaseData("info depth 245 seldepth 11 multipv 1 score mate -5 nodes 12403870 nps 4963533 hashfull 32 tbhits 0 time 2499 pv d2c2 e4d4 c2b2 d4c4 b2a2 g3g2 a2a1 c4c3 a1b1 g2b2")
                    .Returns(245);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetMoveCases))]
        public string TestGetMove(string line)
        {
            return PositionAnalysisNodeExtraction.GetMove(line);
        }

        public static IEnumerable GetMoveCases
        {
            get
            {
                yield return new TestCaseData(
                    "info depth 2 seldepth 3 multipv 24 score cp -644 nodes 1406 nps 468666 hashfull 0 tbhits 0 time 3 pv e4f5 f7f5")
                    .Returns("e4f5");
                yield return new TestCaseData(
                    "info depth 25 seldepth 50 multipv 3 score cp 317 nodes 15966870 nps 787088 hashfull 998 tbhits 0 time 20286 pv g2g4 f6f5 d3f5 a3c4 f1e2 f7f6 e2d3 c4d6 d3c3 e7d8 f5d3 d8c7 b4b5 d6c8 d3f5 c8b6 c3d3 b6d5 f5e4 d5f4 d3c4 f4h3 f2f3 h3f4 c4c5 f4e6 c5d5 e6f4 d5c4 c7b6 e4f5 f4h3 c4d5 b6b5 f5d3 b5b6")
                    .Returns("g2g4");
                yield return new TestCaseData(
                    "info depth 52 seldepth 10 multipv 1 score mate 5 nodes 16000315 nps 3213559 hashfull 192 tbhits 0 time 4979 pv g3a3 c2b1 e4e3 b1c2 a3b4 c2c1 e3d3 c1d1 b4d2")
                    .Returns("g3a3");
                yield return new TestCaseData(
                    "info depth 245 seldepth 11 multipv 1 score mate -5 nodes 12403870 nps 4963533 hashfull 32 tbhits 0 time 2499 pv d2c2 e4d4 c2b2 d4c4 b2a2 g3g2 a2a1 c4c3 a1b1 g2b2")
                    .Returns("d2c2");
            }
        }

    }
}
