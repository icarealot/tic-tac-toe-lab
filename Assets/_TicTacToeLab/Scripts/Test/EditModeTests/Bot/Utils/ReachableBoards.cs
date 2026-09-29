using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public static class ReachableBoards
    {
        public static IEnumerable<BoardModel> InProgressOToMove()
        {
            HashSet<string> seenPositions = new();
            Stack<CellCoordinate[]> pendingPaths = new();
            CellCoordinate[] emptyPath = Array.Empty<CellCoordinate>();
            pendingPaths.Push(emptyPath);
            _ = seenPositions.Add(BotBoards.Encode(BotBoards.Arrange(emptyPath)));

            while (pendingPaths.Count > 0)
            {
                CellCoordinate[] path = pendingPaths.Pop();
                BoardModel boardModel = BotBoards.Arrange(path);

                if (boardModel.Outcome != Outcome.InProgress)
                {
                    continue;
                }

                if (boardModel.Turn == Mark.O)
                {
                    yield return boardModel;
                }

                for (int row = 0; row < boardModel.Dimension; row++)
                {
                    for (int column = 0; column < boardModel.Dimension; column++)
                    {
                        CellCoordinate coordinate = new(row, column);

                        if (!boardModel.IsEmpty(coordinate))
                        {
                            continue;
                        }

                        CellCoordinate[] childPath = BotBoards.Append(path, coordinate);

                        if (seenPositions.Add(BotBoards.Encode(BotBoards.Arrange(childPath))))
                        {
                            pendingPaths.Push(childPath);
                        }
                    }
                }
            }
        }
    }
}
