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
            _inputService.RaisePress(cellCenter);
        }
    }

    public static class BoardMoves
    {
        public static void WinRowZeroForX(BoardPresser presser)
        {
            (int Row, int Column)[] _xWinsRowZero = { (0, 0), (1, 0), (0, 1), (1, 1), (0, 2) };
            PressAll(presser, _xWinsRowZero);
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
