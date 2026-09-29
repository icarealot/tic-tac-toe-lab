namespace TicTacToeLab.Runtime
{
    internal static class BoardLines
    {
        public static bool HasCompleteLine(Mark?[,] marks, Mark mark)
        {
            int dimension = marks.GetLength(0);

            for (int index = 0; index < dimension; index++)
            {
                if (IsRowComplete(marks, index, mark) || IsColumnComplete(marks, index, mark))
                {
                    return true;
                }
            }

            return IsMainDiagonalComplete(marks, mark) || IsAntiDiagonalComplete(marks, mark);
        }

        private static bool IsRowComplete(Mark?[,] marks, int row, Mark mark)
        {
            bool isComplete = true;

            for (int column = 0; column < marks.GetLength(1); column++)
            {
                isComplete = isComplete && marks[row, column] == mark;
            }

            return isComplete;
        }

        private static bool IsColumnComplete(Mark?[,] marks, int column, Mark mark)
        {
            bool isComplete = true;

            for (int row = 0; row < marks.GetLength(0); row++)
            {
                isComplete = isComplete && marks[row, column] == mark;
            }

            return isComplete;
        }

        private static bool IsMainDiagonalComplete(Mark?[,] marks, Mark mark)
        {
            bool isComplete = true;

            for (int index = 0; index < marks.GetLength(0); index++)
            {
                isComplete = isComplete && marks[index, index] == mark;
            }

            return isComplete;
        }

        private static bool IsAntiDiagonalComplete(Mark?[,] marks, Mark mark)
        {
            bool isComplete = true;

            for (int index = 0; index < marks.GetLength(0); index++)
            {
                isComplete = isComplete && marks[index, marks.GetLength(1) - 1 - index] == mark;
            }

            return isComplete;
        }
    }
}
