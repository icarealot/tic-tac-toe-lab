using System;
using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public sealed class BoardModel
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

        public Mark?[,] CopyMarks()
        {
            return (Mark?[,])_marks.Clone();
        }

        public IEnumerable<CellCoordinate> EnumerateEmptyCoordinates()
        {
            for (int row = 0; row < DIMENSION; row++)
            {
                for (int column = 0; column < DIMENSION; column++)
                {
                    CellCoordinate coordinate = new(row, column);

                    if (IsEmpty(coordinate))
                    {
                        yield return coordinate;
                    }
                }
            }
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

            if (BoardLines.HasCompleteLine(_marks, Turn))
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
            foreach (Mark? mark in _marks)
            {
                if (mark == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
