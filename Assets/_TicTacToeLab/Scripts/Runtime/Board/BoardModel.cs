using System.Collections.Generic;
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

        public Mark Turn { get; private set; } = Mark.X;

        public Outcome Outcome { get; private set; } = Outcome.InProgress;

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

        public void PlaceMark(int row, int column)
        {
            _marks[row, column] = Turn;

            if (HasWonLine())
            {
                Outcome = Outcome.Win;
            }
            else if (IsFull())
            {
                Outcome = Outcome.Draw;
            }

            if (Outcome == Outcome.InProgress)
            {
                Turn = Turn == Mark.X ? Mark.O : Mark.X;
            }
        }

        private bool IsFull()
        {
            for (int row = 0; row < DIMENSION; row++)
            {
                for (int column = 0; column < DIMENSION; column++)
                {
                    if (IsEmpty(row, column))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool HasWonLine()
        {
            for (int index = 0; index < DIMENSION; index++)
            {
                if (IsLineWon(RowCells(index)) || IsLineWon(ColumnCells(index)))
                {
                    return true;
                }
            }

            return IsLineWon(MainDiagonalCells()) || IsLineWon(AntiDiagonalCells());
        }

        private bool IsLineWon(IEnumerable<Mark?> line)
        {
            Mark? firstMark = null;
            bool hasFirstMark = false;

            foreach (Mark? mark in line)
            {
                if (mark == null)
                {
                    return false;
                }

                if (!hasFirstMark)
                {
                    firstMark = mark;
                    hasFirstMark = true;
                }
                else if (mark != firstMark)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<Mark?> RowCells(int row)
        {
            for (int column = 0; column < DIMENSION; column++)
            {
                yield return _marks[row, column];
            }
        }

        private IEnumerable<Mark?> ColumnCells(int column)
        {
            for (int row = 0; row < DIMENSION; row++)
            {
                yield return _marks[row, column];
            }
        }

        private IEnumerable<Mark?> MainDiagonalCells()
        {
            for (int index = 0; index < DIMENSION; index++)
            {
                yield return _marks[index, index];
            }
        }

        private IEnumerable<Mark?> AntiDiagonalCells()
        {
            for (int index = 0; index < DIMENSION; index++)
            {
                yield return _marks[index, DIMENSION - 1 - index];
            }
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
