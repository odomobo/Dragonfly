using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.MoveGeneration;
using NUnit.Framework;

namespace Dragonfly.Engine.Tests
{
    [TestFixture]
    class BitboardSquaresTests
    {
        private MoveGen _moveGen;

        [SetUp]
        public void Setup()
        {
            _moveGen = new MoveGen();
        }

        // TODO: different data source??? Maybe rename them?
        [TestCaseSource(typeof(ZobristData), nameof(ZobristData.ZobristTestCases))]
        public void BitboardSquaresTest(string fen, int depth)
        {
            var board = BoardParsing.BoardFromFen(fen);
            BitboardSquaresTestHelper(board, depth);
        }

        private void BitboardSquaresTestHelper(Position position, int depth)
        {
            Assert.DoesNotThrow(position.AssertBitboardsMatchSquares);
            if (depth <= 0)
                return;

            var moves = new List<Move>();
            _moveGen.Generate(moves, position);

            var tmpPosition = new Position();
            foreach (var move in moves)
            {
                var updatedBoard = Position.MakeMove(tmpPosition, move, position);

                if (!_moveGen.OnlyLegalMoves && updatedBoard.InCheck(updatedBoard.SideToMove.Other()))
                    continue;

                BitboardSquaresTestHelper(updatedBoard, depth - 1);
            }
        }
    }
}
