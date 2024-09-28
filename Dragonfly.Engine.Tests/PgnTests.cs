using Dragonfly.Engine.Interfaces;
using Dragonfly.Engine.MoveGeneration;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dragonfly.Engine.Tests
{
    [TestFixture]
    public class PgnTests
    {
        private IMoveGenerator _moveGenerator;

        [SetUp]
        public void Setup()
        {
            _moveGenerator = new MoveGenerator();
        }

        private static readonly Regex ResultRegex = new Regex(@" (1-0|1/2-1/2|0-1)$");
        [TestCaseSource(typeof(PgnData), nameof(PgnData.PgnGames))]
        public void PgnTest(string pgnInStr)
        {
            var pgnObject = Pgn.ParsePgn(_moveGenerator, pgnInStr);
            var pgnOutStr = pgnObject.ToPgnString(_moveGenerator);

            // get rid of all newlines, which PGNs might not agree on. Instead, replace all newlines with spaces
            var massagedPgnInStr = string.Join(" ", pgnInStr.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
            // remove result, which our pgn generator does not include
            massagedPgnInStr = ResultRegex.Replace(massagedPgnInStr, "");
            var massagedPgnOutStr = string.Join(" ", pgnOutStr.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));

            Assert.AreEqual(massagedPgnInStr, massagedPgnOutStr);
        }
    }

    public class PgnData
    {
        public static IEnumerable PgnGames
        {
            get
            {
                foreach (var pgn in Enumerators.GetPgnsEnumerator())
                {
                    yield return new TestCaseData(pgn);
                }
            }
        }
    }
}
