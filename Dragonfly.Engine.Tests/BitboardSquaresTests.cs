using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.MoveGeneration;
using Dragonfly.Engine.PerformanceTypes;
using NUnit.Framework;

namespace Dragonfly.Engine.Tests
{
    [TestFixture]
    class BitboardSquaresTests
    {
        private MoveGenerator _moveGenerator;

        [SetUp]
        public void Setup()
        {
            _moveGenerator = new MoveGenerator();
        }

        // TODO: different data source??? Maybe rename them?
        [TestCaseSource(typeof(ZobristData), nameof(ZobristData.ZobristTestCases))]
        public void BitboardSquaresTest(string fen, int depth)
        {
            var position = BoardParsing.PositionFromFen(fen);
            BitboardSquaresTestHelper(position, depth);
        }

        private void BitboardSquaresTestHelper(Position position, int depth)
        {
            Assert.DoesNotThrow(position.AssertBitboardsMatchSquares);
            if (depth <= 0)
                return;

            var moves = new List<Move>();
            _moveGenerator.Generate(moves, position);

            foreach (var move in moves)
            {
                var updatedBoard = position.MakeMove(move);

                if (!_moveGenerator.OnlyLegalMoves && updatedBoard.MovedIntoCheck())
                    continue;

                BitboardSquaresTestHelper(updatedBoard, depth - 1);
            }
        }
    }
}
