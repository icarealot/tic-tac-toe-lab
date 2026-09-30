using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public sealed class AmateurBot : IBot
    {
        private readonly IRandomService _randomService;

        public AmateurBot(IRandomService randomService)
        {
            _randomService = randomService;
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

            return candidates[_randomService.Range(0, candidates.Count)];
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
