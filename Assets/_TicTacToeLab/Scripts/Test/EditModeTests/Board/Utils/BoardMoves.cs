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
        private static readonly (int Row, int Column)[] _drawFillOrder =
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

        private static readonly (int Row, int Column)[] _xWinsRowZero =
        {
            (0, 0),
            (1, 0),
            (0, 1),
            (1, 1),
            (0, 2),
        };

        private static readonly (int Row, int Column)[] _oWindsRowTwo =
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
            PressAll(presser, _xWinsRowZero);
        }

        public static void WinRowTwoForO(BoardPresser presser)
        {
            PressAll(presser, _oWindsRowTwo);
        }

        public static void PressToDraw(BoardPresser presser)
        {
            PressAll(presser, _drawFillOrder);
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
