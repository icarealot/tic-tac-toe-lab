using System;
using System.Text;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public static class BotBoards
    {
        public static BoardModel Arrange(params CellCoordinate[] placements)
        {
            BoardModel boardModel = new();

            foreach (CellCoordinate coordinate in placements)
            {
                bool accepted = boardModel.TryPlaceMark(coordinate);
                Assert.That(accepted, Is.True, $"The fixture should accept the placement ({coordinate.Row}, {coordinate.Column}).");
            }

            return boardModel;
        }

        public static CellCoordinate[] Append(CellCoordinate[] path, CellCoordinate coordinate)
        {
            CellCoordinate[] childPath = new CellCoordinate[path.Length + 1];
            Array.Copy(path, childPath, path.Length);
            childPath[path.Length] = coordinate;
            return childPath;
        }

        public static string Encode(BoardModel boardModel)
        {
            StringBuilder builder = new(boardModel.Dimension * boardModel.Dimension);

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    Mark? mark = boardModel.GetMark(new CellCoordinate(row, column));
                    _ = builder.Append(mark switch
                    {
                        Mark.X => 'X',
                        Mark.O => 'O',
                        _ => '.',
                    });
                }
            }

            return builder.ToString();
        }
    }
}
