using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BotContractTests
    {
        [Test]
        public void Every_reachable_O_turn_position_returns_an_in_bounds_empty_cell_without_mutating_the_board()
        {
            // Arrange
            FakeRandomService randomService = new();
            IBot[] selectors =
            {
                new AmateurBot(randomService),
                new ProfessionalBot(randomService),
            };

            foreach (BoardModel boardModel in ReachableBoards.InProgressOToMove())
            {
                string position = BotBoards.Encode(boardModel);

                foreach (IBot selector in selectors)
                {
                    Mark turnBefore = boardModel.Turn;
                    Outcome outcomeBefore = boardModel.Outcome;
                    Mark?[,] marksBefore = BoardState.CaptureMarks(boardModel);

                    for (int index = 0; index < boardModel.Dimension * boardModel.Dimension; index++)
                    {
                        randomService.IntegerResult = index;

                        // Act
                        CellCoordinate coordinate = selector.SelectPlacement(boardModel);

                        // Assert
                        string selectorName = selector.GetType().Name;
                        Assert.That(coordinate.IsWithin(boardModel.Dimension), Is.True, $"{selectorName} returned the out-of-bounds cell ({coordinate.Row}, {coordinate.Column}) for position {position} at random index {index}.");
                        Assert.That(boardModel.IsEmpty(coordinate), Is.True, $"{selectorName} returned the marked cell ({coordinate.Row}, {coordinate.Column}) for position {position} at random index {index}.");
                        Assert.That(boardModel.Turn, Is.EqualTo(turnBefore), $"{selectorName} changed the turn for position {position} at random index {index}.");
                        Assert.That(boardModel.Outcome, Is.EqualTo(outcomeBefore), $"{selectorName} changed the outcome for position {position} at random index {index}.");
                        Assert.That(BoardState.CaptureMarks(boardModel), Is.EqualTo(marksBefore), $"{selectorName} changed the marks for position {position} at random index {index}.");
                    }
                }
            }
        }
    }
}
