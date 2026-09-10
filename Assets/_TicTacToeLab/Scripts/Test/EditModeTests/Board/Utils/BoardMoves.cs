using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardPresser
    {
        private readonly FakeInputService _inputService;
        private readonly BoardModel _boardModel;

        public BoardPresser(FakeInputService inputService, BoardModel boardModel)
        {
            _inputService = inputService;
            _boardModel = boardModel;
        }

        public void Press(int row, int column)
        {
            Vector3 cellCenter = _boardModel.GetCellLocalPoint(row, column);
            _inputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));
        }
    }

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

        private static readonly (int Row, int Column)[] X_WINS_ROW_ZERO =
        {
            (0, 0),
            (1, 0),
            (0, 1),
            (1, 1),
            (0, 2),
        };

        private static readonly (int Row, int Column)[] O_WINS_ROW_TWO =
        {
            (0, 0),
            (2, 0),
            (0, 1),
            (2, 1),
            (1, 1),
            (2, 2),
        };

        public static void WinRowZeroForX(BoardPresser presser)
        {
            PressAll(presser, X_WINS_ROW_ZERO);
        }

        public static void WinRowTwoForO(BoardPresser presser)
        {
            PressAll(presser, O_WINS_ROW_TWO);
        }

        public static void PressToDraw(BoardPresser presser)
        {
            PressAll(presser, DRAW_FILL_ORDER);
        }

        public static void FillToDraw(BoardModel boardModel)
        {
            foreach ((int row, int column) in DRAW_FILL_ORDER)
            {
                boardModel.PlaceMark(row, column);
            }
        }

        private static void PressAll(BoardPresser presser, (int Row, int Column)[] moves)
        {
            foreach ((int row, int column) in moves)
            {
                presser.Press(row, column);
            }
        }
    }
}
