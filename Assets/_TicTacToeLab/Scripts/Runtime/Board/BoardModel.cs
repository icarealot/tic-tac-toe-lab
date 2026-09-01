using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardModel
    {
        private const int DIMENSION = 3;
        private const float CELL_SIZE = 1f;
        private const float CELL_SPACING = 0.2f;
        private const float CELL_STEP = CELL_SIZE + CELL_SPACING;
        private const float CENTER_OFFSET = (DIMENSION - 1) * CELL_STEP * 0.5f;

        public int Dimension => DIMENSION;

        private readonly Mark?[,] _marks = new Mark?[DIMENSION, DIMENSION];

        public Vector3 GetCellLocalPoint(int row, int column)
        {
            return new Vector3(
                column * CELL_STEP - CENTER_OFFSET,
                CENTER_OFFSET - row * CELL_STEP,
                0f);
        }

        public bool TryResolveCell(Vector3 localPoint, out int row, out int column)
        {
            row = 0;
            column = 0;
            return TryResolveAxis(localPoint.x, out column) && TryResolveAxis(-localPoint.y, out row);
        }

        public bool IsEmpty(int row, int column)
        {
            return _marks[row, column] == null;
        }

        public Mark? GetMark(int row, int column)
        {
            return _marks[row, column];
        }

        public void PlaceMark(int row, int column, Mark mark)
        {
            _marks[row, column] = mark;
        }

        private bool TryResolveAxis(float value, out int axisValue)
        {
            axisValue = Mathf.RoundToInt((value + CENTER_OFFSET) / CELL_STEP);

            if (axisValue < 0 || axisValue >= DIMENSION)
            {
                return false;
            }

            float cellCenter = axisValue * CELL_STEP - CENTER_OFFSET;
            return Mathf.Abs(value - cellCenter) <= CELL_SIZE * 0.5f;
        }
    }
}
