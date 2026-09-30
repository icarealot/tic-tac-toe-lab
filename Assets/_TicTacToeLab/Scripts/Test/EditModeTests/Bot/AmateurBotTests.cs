using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class AmateurBotTests
    {
        // --- Immediate O win ---

        [Test]
        public void An_immediate_O_win_is_chosen_over_blocking_an_immediate_X_win()
        {
            // Arrange
            // . O O
            // X X .
            // . . X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(1, 0), // X
                new CellCoordinate(0, 1), // O
                new CellCoordinate(1, 1), // X
                new CellCoordinate(0, 2), // O
                new CellCoordinate(2, 2)); // X
            FakeRandomService randomService = new();
            AmateurBot sut = new(randomService);

            // Act
            CellCoordinate coordinate = sut.SelectPlacement(boardModel);

            // Assert
            Assert.That(coordinate, Is.EqualTo(new CellCoordinate(0, 0)), "O should win at (0, 0) instead of blocking the X threat at (1, 2).");
            Assert.That(randomService.LastIntegerMaximumExclusive, Is.EqualTo(1), "Only the immediate winning cell should be a candidate.");
            Assert.That(randomService.LastIntegerMinimumInclusive, Is.EqualTo(0));
        }
        // --- Immediate X win blocking ---

        [Test]
        public void An_immediate_X_win_is_blocked_when_O_cannot_win_immediately()
        {
            // Arrange
            // X X .
            // O . X
            // O . .
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(0, 0), // X
                new CellCoordinate(1, 0), // O
                new CellCoordinate(0, 1), // X
                new CellCoordinate(2, 0), // O
                new CellCoordinate(1, 2)); // X
            FakeRandomService randomService = new();
            AmateurBot sut = new(randomService);

            // Act
            CellCoordinate coordinate = sut.SelectPlacement(boardModel);

            // Assert
            Assert.That(coordinate, Is.EqualTo(new CellCoordinate(0, 2)), "Only (0, 2) blocks the X threat in row 0.");
            Assert.That(randomService.LastIntegerMaximumExclusive, Is.EqualTo(1), "Only the blocking cell should be a candidate.");
            Assert.That(randomService.LastIntegerMinimumInclusive, Is.EqualTo(0));
        }

        // --- Random fallback ---

        [Test]
        public void An_empty_cell_is_chosen_randomly_when_neither_side_can_win_immediately()
        {
            // Arrange
            // X . .
            // . O .
            // . . X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(0, 0), // X
                new CellCoordinate(1, 1), // O
                new CellCoordinate(2, 2)); // X
            CellCoordinate[] emptyCoordinates =
            {
                new(0, 1),
                new(0, 2),
                new(1, 0),
                new(1, 2),
                new(2, 0),
                new(2, 1),
            };
            FakeRandomService randomService = new();
            AmateurBot sut = new(randomService);
            HashSet<CellCoordinate> chosenCoordinates = new();

            // Act
            for (int index = 0; index < emptyCoordinates.Length; index++)
            {
                randomService.IntegerResult = index;
                _ = chosenCoordinates.Add(sut.SelectPlacement(boardModel));
            }

            // Assert
            Assert.That(randomService.LastIntegerMaximumExclusive, Is.EqualTo(emptyCoordinates.Length), "Every empty cell should be a candidate.");
            Assert.That(randomService.LastIntegerMinimumInclusive, Is.EqualTo(0));
            Assert.That(chosenCoordinates, Is.EquivalentTo(emptyCoordinates), "Each controlled random index should select a different empty cell.");
        }

        // --- Multiple candidates within the top priority ---

        [Test]
        public void Only_immediate_O_wins_are_candidates_when_several_exist()
        {
            // Arrange
            // O O .
            // X O X
            // X X .
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(1, 0), // X
                new CellCoordinate(0, 0), // O
                new CellCoordinate(1, 2), // X
                new CellCoordinate(0, 1), // O
                new CellCoordinate(2, 0), // X
                new CellCoordinate(1, 1), // O
                new CellCoordinate(2, 1)); // X
            CellCoordinate[] winningCoordinates =
            {
                new(0, 2),
                new(2, 2),
            };
            FakeRandomService randomService = new();
            AmateurBot sut = new(randomService);
            HashSet<CellCoordinate> chosenCoordinates = new();

            // Act
            for (int index = 0; index < 8; index++)
            {
                randomService.IntegerResult = index;
                _ = chosenCoordinates.Add(sut.SelectPlacement(boardModel));
            }

            // Assert
            Assert.That(randomService.LastIntegerMaximumExclusive, Is.EqualTo(winningCoordinates.Length), "Only the two immediate O wins should be candidates.");
            Assert.That(randomService.LastIntegerMinimumInclusive, Is.EqualTo(0));
            Assert.That(chosenCoordinates, Is.EquivalentTo(winningCoordinates), "Only the two immediate O wins should be selected.");
        }

        [Test]
        public void Only_immediate_X_wins_are_blocked_when_several_exist()
        {
            // Arrange
            // X X .
            // X O .
            // . . O
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(0, 0), // X
                new CellCoordinate(1, 1), // O
                new CellCoordinate(0, 1), // X
                new CellCoordinate(2, 2), // O
                new CellCoordinate(1, 0)); // X
            CellCoordinate[] blockingCoordinates =
            {
                new(0, 2),
                new(2, 0),
            };
            FakeRandomService randomService = new();
            AmateurBot sut = new(randomService);
            HashSet<CellCoordinate> chosenCoordinates = new();

            // Act
            for (int index = 0; index < 8; index++)
            {
                randomService.IntegerResult = index;
                _ = chosenCoordinates.Add(sut.SelectPlacement(boardModel));
            }

            // Assert
            Assert.That(randomService.LastIntegerMaximumExclusive, Is.EqualTo(blockingCoordinates.Length), "Only the two blocking cells should be candidates.");
            Assert.That(randomService.LastIntegerMinimumInclusive, Is.EqualTo(0));
            Assert.That(chosenCoordinates, Is.EquivalentTo(blockingCoordinates), "Only the two blocking cells should be selected, never an unrelated empty cell.");
        }
    }
}
