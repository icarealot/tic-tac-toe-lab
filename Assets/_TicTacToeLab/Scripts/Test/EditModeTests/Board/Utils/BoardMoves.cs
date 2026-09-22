using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardPresser
    {
        private readonly FakeInputService _inputService;
        private readonly BoardLayout _boardLayout;

        public BoardPresser(FakeInputService inputService, BoardLayout boardLayout)
        {
            _inputService = inputService;
            _boardLayout = boardLayout;
        }

        public void Press(CellCoordinate coordinate)
        {
            Vector3 cellCenter = _boardLayout.GetCellLocalPoint(coordinate);
            _inputService.RaisePress(cellCenter);
        }
    }

    public static class BoardMoves
    {
        public static void WinRowZeroForX(BoardPresser presser)
        {
            CellCoordinate[] _xWinsRowZero = { new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(0, 2) };
            PressAll(presser, _xWinsRowZero);
        }

        public static void WinRowZeroForO(BoardPresser presser)
        {
            CellCoordinate[] _oWinsRowZero = { new(1, 0), new(0, 0), new(1, 1), new(0, 1), new(2, 2), new(0, 2) };
            PressAll(presser, _oWinsRowZero);
        }

        public static void FillForDraw(BoardPresser presser)
        {
            CellCoordinate[] _drawFillOrder = { new(0, 0), new(0, 1), new(0, 2), new(1, 1), new(1, 0), new(1, 2), new(2, 1), new(2, 0), new(2, 2) };
            PressAll(presser, _drawFillOrder);
        }

        private static void PressAll(BoardPresser presser, CellCoordinate[] moves)
        {
            foreach (CellCoordinate coordinate in moves)
            {
                presser.Press(coordinate);
            }
        }
    }
}
