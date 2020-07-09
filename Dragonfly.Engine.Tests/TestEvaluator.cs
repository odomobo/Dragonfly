using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Evaluation;
using NUnit.Framework;

namespace Dragonfly.Engine.Tests
{
    [TestFixture]
    class TestEvaluator
    {
        [TestCaseSource(typeof(IndexData), nameof(IndexData.IndexTestCases))]
        public void TestIxForColor_VerifySwapRankOnBlack(int file, int rank)
        {
            var origIx = Position.IxFromFileRank(file, rank);
            var actual = Evaluator.IxForColor(origIx, Color.Black);

            var expected = Position.IxFromFileRank(file, 7 - rank);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(typeof(IndexData), nameof(IndexData.IndexTestCases))]
        public void TestIxForColor_VerifyIdentityOnWhite(int file, int rank)
        {
            var origIx = Position.IxFromFileRank(file, rank);
            var actual = Evaluator.IxForColor(origIx, Color.White);

            Assert.AreEqual(origIx, actual);
        }
    }

    public class IndexData
    {
        public static IEnumerable IndexTestCases
        {
            get
            {
                foreach (var (file, rank) in Position.GetAllFilesRanks())
                {
                    yield return new TestCaseData(file, rank);
                }
            }
        }
    }
}
