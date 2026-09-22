using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public static class BoardState
    {
        public static Mark?[,] CaptureMarks(BoardModel boardModel)
        {
            Mark?[,] marks = new Mark?[boardModel.Dimension, boardModel.Dimension];

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    marks[row, column] = boardModel.GetMark(new CellCoordinate(row, column));
                }
            }

            return marks;
        }

        public static bool IsEmpty(BoardModel boardModel)
        {
            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    if (!boardModel.IsEmpty(new CellCoordinate(row, column)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
