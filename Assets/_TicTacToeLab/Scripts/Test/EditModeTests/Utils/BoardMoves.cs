using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public static class BoardMoves
    {
        // This order fills the board without ever completing a row, column or diagonal.
        private static readonly (int Row, int Column)[] DRAW_FILL_ORDER =
        {
            (0, 0),
            (0, 1),
            (0, 2),
            (1, 1),
            (1, 0),
            (1, 2),
            (2, 1),
            (2, 0),
            (2, 2),
        };

        public static void PressCell(FakeInputService fakeInputService, BoardModel boardModel, int row, int column)
        {
            Vector3 cellCenter = boardModel.GetCellLocalPoint(row, column);
            fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));
        }

        public static void WinRowZeroForX(FakeInputService fakeInputService, BoardModel boardModel)
        {
            PressCell(fakeInputService, boardModel, 0, 0); // X
            PressCell(fakeInputService, boardModel, 1, 0); // O
            PressCell(fakeInputService, boardModel, 0, 1); // X
            PressCell(fakeInputService, boardModel, 1, 1); // O
            PressCell(fakeInputService, boardModel, 0, 2); // X completes row 0
        }

        public static void PressToDraw(FakeInputService fakeInputService, BoardModel boardModel)
        {
            foreach ((int row, int column) in DRAW_FILL_ORDER)
            {
                PressCell(fakeInputService, boardModel, row, column);
            }
        }

        public static void FillToDraw(BoardModel boardModel)
        {
            foreach ((int row, int column) in DRAW_FILL_ORDER)
            {
                boardModel.PlaceMark(row, column);
            }
        }
    }
}
