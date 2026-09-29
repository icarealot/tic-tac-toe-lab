using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public sealed class AmateurBot : IBot
    {
        private readonly IRandomChoiceSource _randomChoiceSource;

        public AmateurBot(IRandomChoiceSource randomChoiceSource)
        {
            _randomChoiceSource = randomChoiceSource;
        }

        public CellCoordinate SelectPlacement(BoardModel boardModel)
        {
            Mark?[,] marks = boardModel.CopyMarks();
            List<CellCoordinate> emptyCoordinates = new(boardModel.EnumerateEmptyCoordinates());
            List<CellCoordinate> candidates = CollectCompletingCoordinates(marks, emptyCoordinates, Mark.O);

            if (candidates.Count == 0)
            {
                candidates = CollectCompletingCoordinates(marks, emptyCoordinates, Mark.X);
            }

            if (candidates.Count == 0)
            {
                candidates = emptyCoordinates;
            }

            return candidates[_randomChoiceSource.NextIndex(candidates.Count)];
        }

        private static List<CellCoordinate> CollectCompletingCoordinates(Mark?[,] marks, List<CellCoordinate> emptyCoordinates, Mark mark)
        {
            List<CellCoordinate> coordinates = new();

            foreach (CellCoordinate coordinate in emptyCoordinates)
            {
                marks[coordinate.Row, coordinate.Column] = mark;

                if (BoardLines.HasCompleteLine(marks, mark))
                {
                    coordinates.Add(coordinate);
                }

                marks[coordinate.Row, coordinate.Column] = null;
            }

            return coordinates;
        }
    }
}
