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
            for (int column = 0; column < marks.GetLength(1); column++)
            {
                if (marks[row, column] != mark)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsColumnComplete(Mark?[,] marks, int column, Mark mark)
        {
            for (int row = 0; row < marks.GetLength(0); row++)
            {
                if (marks[row, column] != mark)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsMainDiagonalComplete(Mark?[,] marks, Mark mark)
        {
            for (int index = 0; index < marks.GetLength(0); index++)
            {
                if (marks[index, index] != mark)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAntiDiagonalComplete(Mark?[,] marks, Mark mark)
        {
            for (int index = 0; index < marks.GetLength(0); index++)
            {
                if (marks[index, marks.GetLength(1) - 1 - index] != mark)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
