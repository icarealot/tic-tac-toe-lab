using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardViewTests
    {
        private BoardView CreateConstructedBoardView(BoardModel boardModel, IFactoryService factoryService)
        {
            GameObject gameObject = new("Board");
            BoardView boardView = gameObject.AddComponent<BoardView>();

            List<CellPlacement> placements = new();
            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    placements.Add(new CellPlacement(row, column, boardModel.GetCellLocalPoint(row, column)));
                }
            }

            boardView.Construct(factoryService, boardModel.Dimension, placements);
            return boardView;
        }

        [Test]
        public void Clearing_the_board_view_leaves_every_cell_in_place_with_no_mark_showing()
        {
            BoardModel boardModel = new();
            FakeFactoryService fakeFactoryService = new();
            BoardView boardView = CreateConstructedBoardView(boardModel, fakeFactoryService);

            boardView.ShowMark(0, 0, Mark.X);
            boardView.ShowMark(1, 1, Mark.O);
            int cellCountBeforeClear = boardView.GetComponentsInChildren<CellView>().Length;

            boardView.Clear();

            Assert.That(boardView.GetComponentsInChildren<CellView>().Length, Is.EqualTo(cellCountBeforeClear));
            Assert.That(boardView.GetComponentsInChildren<MarkView>(), Is.Empty);
        }
    }
}
