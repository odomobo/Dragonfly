using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.MoveGens;
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

        private void BitboardSquaresTestHelper(Board board, int depth)
        {
            Assert.DoesNotThrow(board.AssertBitboardsMatchSquares);
            if (depth <= 0)
                return;

            var moves = new List<Move>();
            _moveGen.Generate(moves, board);

            foreach (var move in moves)
            {
                var updatedBoard = Board.MakeMove(new Board(), move, board);

                if (!_moveGen.OnlyLegalMoves && updatedBoard.InCheck(updatedBoard.SideToMove.Other()))
                    continue;

                BitboardSquaresTestHelper(updatedBoard, depth - 1);
            }
        }
    }
}
