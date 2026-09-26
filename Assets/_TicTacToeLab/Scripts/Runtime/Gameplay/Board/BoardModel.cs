using System;

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
            bool isFull = true;

            foreach (Mark? mark in _marks)
            {
                isFull = isFull && mark != null;
            }

            return isFull;
        }

        private bool HasWonLine(Mark mark)
        {
            bool hasWonLine = false;

            for (int index = 0; index < DIMENSION; index++)
            {
                hasWonLine = hasWonLine || IsRowComplete(index, mark) || IsColumnComplete(index, mark);
            }

            return hasWonLine || IsMainDiagonalComplete(mark) || IsAntiDiagonalComplete(mark);
        }

        private bool IsRowComplete(int row, Mark mark)
        {
            bool isComplete = true;

            for (int column = 0; column < DIMENSION; column++)
            {
                isComplete = isComplete && _marks[row, column] == mark;
            }

            return isComplete;
        }

        private bool IsColumnComplete(int column, Mark mark)
        {
            bool isComplete = true;

            for (int row = 0; row < DIMENSION; row++)
            {
                isComplete = isComplete && _marks[row, column] == mark;
            }

            return isComplete;
        }

        private bool IsMainDiagonalComplete(Mark mark)
        {
            bool isComplete = true;

            for (int index = 0; index < DIMENSION; index++)
            {
                isComplete = isComplete && _marks[index, index] == mark;
            }

            return isComplete;
        }

        private bool IsAntiDiagonalComplete(Mark mark)
        {
            bool isComplete = true;

            for (int index = 0; index < DIMENSION; index++)
            {
                isComplete = isComplete && _marks[index, DIMENSION - 1 - index] == mark;
            }

            return isComplete;
        }
    }
}
