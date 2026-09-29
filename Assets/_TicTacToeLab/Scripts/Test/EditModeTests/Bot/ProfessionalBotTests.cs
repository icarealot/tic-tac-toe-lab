using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class ProfessionalBotTests
    {
        // --- Forced O win ---

        [Test]
        public void A_forced_O_win_is_chosen_over_drawing_or_losing_alternatives()
        {
            // Arrange
            // . . .
            // . . .
            // O X X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(2, 2), // X
                new CellCoordinate(2, 0), // O
                new CellCoordinate(2, 1)); // X
            CellCoordinate[] forcedWinCoordinates =
            {
                new(0, 0),
                new(1, 0),
            };
            FakeRandomChoiceSource randomChoiceSource = new();
            ProfessionalBot sut = new(randomChoiceSource);
            HashSet<CellCoordinate> chosenCoordinates = new();

            // Act
            for (int index = 0; index < 8; index++)
            {
                randomChoiceSource.Index = index;
                _ = chosenCoordinates.Add(sut.SelectPlacement(boardModel));
            }

            // Assert
            Assert.That(randomChoiceSource.LastCandidateCount, Is.EqualTo(forcedWinCoordinates.Length), "Only the two forced wins should be candidates.");
            Assert.That(chosenCoordinates, Is.EquivalentTo(forcedWinCoordinates), "O should choose a forced win over the drawing and losing alternatives.");
        }

        // --- Earlier O win preferred ---

        [Test]
        public void An_earlier_O_win_is_chosen_over_a_later_O_win()
        {
            // Arrange
            // . . X
            // . O .
            // X O X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(2, 2), // X
                new CellCoordinate(2, 1), // O
                new CellCoordinate(2, 0), // X
                new CellCoordinate(1, 1), // O
                new CellCoordinate(0, 2)); // X
            FakeRandomChoiceSource randomChoiceSource = new();
            ProfessionalBot sut = new(randomChoiceSource);

            // Act
            CellCoordinate coordinate = sut.SelectPlacement(boardModel);

            // Assert
            Assert.That(coordinate, Is.EqualTo(new CellCoordinate(0, 1)), "O should win immediately at (0, 1) instead of forcing a later win at (1, 2).");
            Assert.That(randomChoiceSource.LastCandidateCount, Is.EqualTo(1), "Only the earlier win should be a candidate.");
        }

        // --- Unavoidable X win delayed ---

        [Test]
        public void An_unavoidable_X_win_is_delayed_as_long_as_possible()
        {
            // Arrange
            // . . .
            // . . X
            // . O X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(2, 2), // X
                new CellCoordinate(2, 1), // O
                new CellCoordinate(1, 2)); // X
            FakeRandomChoiceSource randomChoiceSource = new();
            ProfessionalBot sut = new(randomChoiceSource);

            // Act
            CellCoordinate coordinate = sut.SelectPlacement(boardModel);

            // Assert
            Assert.That(coordinate, Is.EqualTo(new CellCoordinate(0, 2)), "O should play (0, 2) to delay the unavoidable X win.");
            Assert.That(randomChoiceSource.LastCandidateCount, Is.EqualTo(1), "Only the delaying cell should be a candidate.");
        }

        // --- Draw over loss ---

        [Test]
        public void A_draw_is_chosen_over_an_unavoidable_loss()
        {
            // Arrange
            // . . .
            // . . .
            // . . X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(2, 2)); // X
            FakeRandomChoiceSource randomChoiceSource = new();
            ProfessionalBot sut = new(randomChoiceSource);

            // Act
            CellCoordinate coordinate = sut.SelectPlacement(boardModel);

            // Assert
            Assert.That(coordinate, Is.EqualTo(new CellCoordinate(1, 1)), "O should take the center to draw instead of losing.");
            Assert.That(randomChoiceSource.LastCandidateCount, Is.EqualTo(1), "Only the drawing cell should be a candidate.");
        }

        // --- Equal optimal placements ---

        [Test]
        public void Equal_optimal_drawing_placements_are_resolved_randomly()
        {
            // Arrange
            // . X .
            // . O .
            // X O X
            BoardModel boardModel = BotBoards.Arrange(
                new CellCoordinate(2, 2), // X
                new CellCoordinate(2, 1), // O
                new CellCoordinate(2, 0), // X
                new CellCoordinate(1, 1), // O
                new CellCoordinate(0, 1)); // X
            CellCoordinate[] drawingCoordinates =
            {
                new(0, 0),
                new(0, 2),
                new(1, 0),
                new(1, 2),
            };
            FakeRandomChoiceSource randomChoiceSource = new();
            ProfessionalBot sut = new(randomChoiceSource);
            HashSet<CellCoordinate> chosenCoordinates = new();

            // Act
            for (int index = 0; index < drawingCoordinates.Length; index++)
            {
                randomChoiceSource.Index = index;
                _ = chosenCoordinates.Add(sut.SelectPlacement(boardModel));
            }

            // Assert
            Assert.That(randomChoiceSource.LastCandidateCount, Is.EqualTo(drawingCoordinates.Length), "Every drawing placement should be an equally optimal candidate.");
            Assert.That(chosenCoordinates, Is.EquivalentTo(drawingCoordinates), "Draws should not be ranked by how quickly they end.");
        }

        // --- Complete legal-continuation validation ---

        [Test]
        public void X_cannot_win_from_an_empty_board_against_Professional_including_every_optimal_O_choice()
        {
            // Arrange
            FakeRandomChoiceSource randomChoiceSource = new();
            ProfessionalBot sut = new(randomChoiceSource);
            HashSet<string> exploredPositions = new();
            List<string> xWinPaths = new();

            // Act
            ExploreXTurn(sut, randomChoiceSource, exploredPositions, xWinPaths, Array.Empty<CellCoordinate>());

            // Assert
            string firstXWinPath = xWinPaths.Count > 0 ? xWinPaths[0] : string.Empty;
            Assert.That(xWinPaths, Is.Empty, $"X reached {xWinPaths.Count} winning continuation(s); first: {firstXWinPath}.");
            // The root position and the nine positions after X's first move prove that the traversal examined real games.
            Assert.That(exploredPositions.Count, Is.GreaterThanOrEqualTo(10), "The traversal should have examined the root and every position reached by X's first move.");
        }

        private static void ExploreXTurn(ProfessionalBot sut, FakeRandomChoiceSource randomChoiceSource, HashSet<string> exploredPositions, List<string> xWinPaths, CellCoordinate[] path)
        {
            BoardModel boardModel = BotBoards.Arrange(path);

            if (boardModel.Outcome != Outcome.InProgress)
            {
                return;
            }

            if (!exploredPositions.Add(BotBoards.Encode(boardModel)))
            {
                return;
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
                    BoardModel childBoard = BotBoards.Arrange(childPath);

                    if (childBoard.Outcome == Outcome.XWin)
                    {
                        xWinPaths.Add(Describe(childPath));
                        continue;
                    }

                    if (childBoard.Outcome == Outcome.InProgress)
                    {
                        ExploreOTurn(sut, randomChoiceSource, exploredPositions, xWinPaths, childPath);
                    }
                }
            }
        }

        private static void ExploreOTurn(ProfessionalBot sut, FakeRandomChoiceSource randomChoiceSource, HashSet<string> exploredPositions, List<string> xWinPaths, CellCoordinate[] path)
        {
            BoardModel boardModel = BotBoards.Arrange(path);

            if (!exploredPositions.Add(BotBoards.Encode(boardModel)))
            {
                return;
            }

            foreach (CellCoordinate choice in EnumerateOptimalChoices(sut, randomChoiceSource, boardModel))
            {
                ExploreXTurn(sut, randomChoiceSource, exploredPositions, xWinPaths, BotBoards.Append(path, choice));
            }
        }

        private static List<CellCoordinate> EnumerateOptimalChoices(ProfessionalBot sut, FakeRandomChoiceSource randomChoiceSource, BoardModel boardModel)
        {
            List<CellCoordinate> choices = new();

            for (int index = 0; index < boardModel.Dimension * boardModel.Dimension; index++)
            {
                randomChoiceSource.Index = index;
                CellCoordinate choice = sut.SelectPlacement(boardModel);

                if (!choices.Contains(choice))
                {
                    choices.Add(choice);
                }
            }

            return choices;
        }

        private static string Describe(CellCoordinate[] path)
        {
            List<string> coordinates = new();

            foreach (CellCoordinate coordinate in path)
            {
                coordinates.Add($"({coordinate.Row}, {coordinate.Column})");
            }

            return string.Join(" -> ", coordinates);
        }
    }
}
