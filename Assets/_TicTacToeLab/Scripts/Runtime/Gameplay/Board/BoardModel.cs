using System;
using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public class BoardModel
    {
        private const int DIMENSION = 3;

        public int Dimension => DIMENSION;
        public Mark Turn { get; private set; } = Mark.X;
        public Outcome Outcome { get; private set; } = Outcome.InProgress;

        private readonly Mark?[,] _marks = new Mark?[DIMENSION, DIMENSION];

        public bool IsEmpty(CellCoordinate coordinate)
        {
            return _marks[coordinate.Row, coordinate.Column] == null;
        }

        public Mark? GetMark(CellCoordinate coordinate)
        {
            return _marks[coordinate.Row, coordinate.Column];
        }

        public void Reset()
        {
            Array.Clear(_marks, 0, _marks.Length);
            Turn = Mark.X;
            Outcome = Outcome.InProgress;
        }

        public bool TryPlaceMark(CellCoordinate coordinate)
        {
            if (Outcome != Outcome.InProgress || !coordinate.IsWithin(DIMENSION) || !IsEmpty(coordinate))
            {
                return false;
            }

            _marks[coordinate.Row, coordinate.Column] = Turn;

            if (HasWonLine(Turn))
            {
                Outcome = Turn == Mark.X ? Outcome.XWin : Outcome.OWin;
            }
            else if (IsFull())
            {
                Outcome = Outcome.Draw;
            }

            if (Outcome == Outcome.InProgress)
            {
                Turn = Turn == Mark.X ? Mark.O : Mark.X;
            }

            return true;
        }

        private bool IsFull()
        {
            for (int row = 0; row < DIMENSION; row++)
            {
                for (int column = 0; column < DIMENSION; column++)
                {
                    if (IsEmpty(new CellCoordinate(row, column)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool HasWonLine(Mark mark)
        {
            for (int index = 0; index < DIMENSION; index++)
            {
                if (IsLineComplete(RowCells(index), mark) || IsLineComplete(ColumnCells(index), mark))
                {
                    return true;
                }
            }

            return IsLineComplete(MainDiagonalCells(), mark) || IsLineComplete(AntiDiagonalCells(), mark);
        }

        private static bool IsLineComplete(IEnumerable<Mark?> line, Mark mark)
        {
            foreach (Mark? cellMark in line)
            {
                if (cellMark != mark)
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
    }
}
