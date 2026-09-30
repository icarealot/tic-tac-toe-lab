using System;
using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public sealed class ProfessionalBot : IBot
    {
        private const int WIN_SCORE = 1000;
        private const int LOSS_SCORE = -1000;
        private const int DRAW_SCORE = 0;

        private readonly IRandomService _randomService;

        public ProfessionalBot(IRandomService randomService)
        {
            _randomService = randomService;
        }

        public CellCoordinate SelectPlacement(BoardModel boardModel)
        {
            Mark?[,] marks = boardModel.CopyMarks();
            List<CellCoordinate> bestCoordinates = new();
            int bestScore = int.MinValue;

            foreach (CellCoordinate coordinate in boardModel.EnumerateEmptyCoordinates())
            {
                marks[coordinate.Row, coordinate.Column] = Mark.O;
                int score = BoardLines.HasCompleteLine(marks, Mark.O)
                    ? WIN_SCORE - 1
                    : SearchBestScore(marks, Mark.X, 1);
                marks[coordinate.Row, coordinate.Column] = null;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCoordinates.Clear();
                    bestCoordinates.Add(coordinate);
                }
                else if (score == bestScore)
                {
                    bestCoordinates.Add(coordinate);
                }
            }

            return bestCoordinates[_randomService.Range(0, bestCoordinates.Count)];
        }

        private static int SearchBestScore(Mark?[,] marks, Mark turn, int depth)
        {
            int bestScore = turn == Mark.O ? int.MinValue : int.MaxValue;
            int emptyCount = 0;

            for (int row = 0; row < marks.GetLength(0); row++)
            {
                for (int column = 0; column < marks.GetLength(1); column++)
                {
                    if (marks[row, column] != null)
                    {
                        continue;
                    }

                    emptyCount++;
                    marks[row, column] = turn;
                    int score;

                    if (BoardLines.HasCompleteLine(marks, turn))
                    {
                        score = turn == Mark.O ? WIN_SCORE - (depth + 1) : LOSS_SCORE + (depth + 1);
                    }
                    else
                    {
                        score = SearchBestScore(marks, turn == Mark.O ? Mark.X : Mark.O, depth + 1);
                    }

                    marks[row, column] = null;
                    bestScore = turn == Mark.O ? Math.Max(bestScore, score) : Math.Min(bestScore, score);
                }
            }

            return emptyCount == 0 ? DRAW_SCORE : bestScore;
        }
    }
}
