using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Dragonfly.Engine.MoveGens;

namespace Dragonfly.Engine.Tests
{
    [TestFixture]
    public class FenTests
    {
        [TestCaseSource(typeof(FenData), nameof(FenData.FenTestCases))]
        public string FenTest(string fen)
        {
            var board = BoardParsing.BoardFromFen(fen);
            return BoardParsing.FenStringFromBoard(board);
        }
    }

    public class FenData
    {
        public static IEnumerable FenTestCases
        {
            get
            {
                foreach (var line in Enumerators.GetPerftCasesEnumerator())
                {
                    var splitLine = line.Split(';', 2);
                    var fen = splitLine[0].Trim();
                    yield return new TestCaseData(fen).Returns(fen);
                }
            }
        }
    }
}
