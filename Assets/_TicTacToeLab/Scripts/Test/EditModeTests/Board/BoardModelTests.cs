using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardModelTests
    {
        public sealed class BoardScenario
        {
            public readonly (int Row, int Column)[] Placements;
            public readonly Mark? Winner;

            public BoardScenario((int Row, int Column)[] placements, Mark? winner = null)
            {
                Placements = placements;
                Winner = winner;
            }
        }

        private static readonly BoardScenario _xWinsRowZero = new(new (int Row, int Column)[] { (0, 0), (1, 0), (0, 1), (1, 1), (0, 2) }, Mark.X);
        private static readonly BoardScenario _xWinsRowOne = new(new (int Row, int Column)[] { (1, 0), (0, 0), (1, 1), (0, 1), (1, 2) }, Mark.X);
        private static readonly BoardScenario _xWinsRowTwo = new(new (int Row, int Column)[] { (2, 0), (0, 0), (2, 1), (0, 1), (2, 2) }, Mark.X);
        private static readonly BoardScenario _xWinsColumnZero = new(new (int Row, int Column)[] { (0, 0), (0, 1), (1, 0), (1, 1), (2, 0) }, Mark.X);
        private static readonly BoardScenario _xWinsColumnOne = new(new (int Row, int Column)[] { (0, 1), (0, 0), (1, 1), (1, 0), (2, 1) }, Mark.X);
        private static readonly BoardScenario _xWinsColumnTwo = new(new (int Row, int Column)[] { (0, 2), (0, 0), (1, 2), (1, 0), (2, 2) }, Mark.X);
        private static readonly BoardScenario _xWinsTheMainDiagonal = new(new (int Row, int Column)[] { (0, 0), (0, 1), (1, 1), (0, 2), (2, 2) }, Mark.X);
        private static readonly BoardScenario _xWinsTheAntiDiagonal = new(new (int Row, int Column)[] { (0, 2), (0, 0), (1, 1), (0, 1), (2, 0) }, Mark.X);
        private static readonly BoardScenario _oWinsRowTwo = new(new (int Row, int Column)[] { (0, 0), (2, 0), (0, 1), (2, 1), (1, 0), (2, 2) }, Mark.O);
        private static readonly BoardScenario _leavesRowZeroIncomplete = new(new (int Row, int Column)[] { (0, 0), (1, 0), (0, 1) });
        private static readonly BoardScenario _leavesRowZeroMixed = new(new (int Row, int Column)[] { (0, 0), (0, 1), (1, 0), (1, 1), (2, 1), (0, 2) });
        private static readonly BoardScenario _fillsToDraw = new(new (int Row, int Column)[] { (0, 0), (0, 1), (0, 2), (1, 1), (1, 0), (1, 2), (2, 1), (2, 0), (2, 2) });

        private static IEnumerable<BoardScenario> WinningLines()
        {
            yield return _xWinsRowZero;
            yield return _xWinsRowOne;
            yield return _xWinsRowTwo;
            yield return _xWinsColumnZero;
            yield return _xWinsColumnOne;
            yield return _xWinsColumnTwo;
            yield return _xWinsTheMainDiagonal;
            yield return _xWinsTheAntiDiagonal;
            yield return _oWinsRowTwo;
        }

        private static IEnumerable<BoardScenario> WinsByEachMark()
        {
            yield return _xWinsRowZero;
            yield return _oWinsRowTwo;
        }

        private static IEnumerable<BoardScenario> NonWinningPatterns()
        {
            yield return _leavesRowZeroIncomplete;
            yield return _leavesRowZeroMixed;
        }

        private static IEnumerable<BoardScenario> GamesToReset()
        {
            yield return _leavesRowZeroIncomplete;
            yield return _xWinsRowZero;
            yield return _oWinsRowTwo;
            yield return _fillsToDraw;
        }

        // --- Marks ---

        [Test]
        public void A_placement_records_the_current_turns_mark_only_in_the_addressed_cell()
        {
            // Arrange
            Mark?[,] expectedMarks =
            {
                { null, null, Mark.O },
                { null, Mark.X, null },
                { null, null, null },
            };
            BoardModel sut = new();

            // Act
            sut.PlaceMark(1, 1); // X
            sut.PlaceMark(0, 2); // O

            // Assert
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Mark? actualMark = sut.GetMark(row, column);
                    Mark? expectedMark = expectedMarks[row, column];
                    string message = $"Cell ({row}, {column}) should hold {expectedMarks[row, column]?.ToString() ?? "nothing"}.";
                    Assert.That(actualMark, Is.EqualTo(expectedMark), message);
                }
            }
        }

        // --- Turns ---

        [TestCase(1, Mark.O)]
        [TestCase(2, Mark.X)]
        public void The_turn_advances_to_the_other_mark_after_each_placed_mark_while_the_game_is_in_progress(int placedMarks, Mark expectedTurn)
        {
            BoardModel sut = new();

            for (int index = 0; index < placedMarks; index++)
            {
                sut.PlaceMark(0, index);
            }

            Assert.That(sut.Turn, Is.EqualTo(expectedTurn));
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [TestCaseSource(nameof(WinsByEachMark))]
        public void The_winning_placement_does_not_advance_the_turn_so_the_turn_names_the_winner(BoardScenario game)
        {
            BoardModel sut = new();

            PlayAll(sut, game.Placements);

            Assert.That(sut.Turn, Is.EqualTo(game.Winner));
        }

        [Test]
        public void The_final_drawing_placement_does_not_advance_the_turn()
        {
            BoardModel sut = new();

            PlayAll(sut, _fillsToDraw.Placements);

            Assert.That(sut.Turn, Is.EqualTo(Mark.X)); // X placed the final mark.
        }

        // --- Outcomes ---

        [TestCaseSource(nameof(WinningLines))]
        public void Completing_a_line_wins_the_game(BoardScenario game)
        {
            BoardModel sut = new();

            PlayAll(sut, game.Placements);

            Assert.That(sut.Outcome, Is.EqualTo(Outcome.Win));
        }

        [TestCaseSource(nameof(NonWinningPatterns))]
        public void A_pattern_that_completes_no_line_leaves_the_game_in_progress(BoardScenario pattern)
        {
            BoardModel sut = new();

            PlayAll(sut, pattern.Placements);

            Assert.That(sut.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void A_board_filled_without_completing_a_line_is_a_draw()
        {
            BoardModel sut = new();

            PlayAll(sut, _fillsToDraw.Placements);

            Assert.That(sut.Outcome, Is.EqualTo(Outcome.Draw));
        }

        [Test]
        public void A_win_on_the_last_empty_cell_is_a_win_not_a_draw()
        {
            (int Row, int Column)[] winOnTheLastEmptyCell = { (0, 0), (2, 0), (0, 1), (1, 1), (1, 0), (1, 2), (2, 1), (2, 2), (0, 2) };
            BoardModel sut = new();

            PlayAll(sut, winOnTheLastEmptyCell);

            Assert.That(sut.Outcome, Is.EqualTo(Outcome.Win));
        }

        // --- Reset ---

        [TestCaseSource(nameof(GamesToReset))]
        public void Resetting_a_played_board_leaves_the_same_public_state_as_a_fresh_board(BoardScenario game)
        {
            // Arrange
            BoardModel sut = new();
            PlayAll(sut, game.Placements);
            BoardModel freshBoard = new();

            // Act
            sut.Reset();

            // Assert
            Assert.That(sut.Turn, Is.EqualTo(freshBoard.Turn));
            Assert.That(sut.Outcome, Is.EqualTo(freshBoard.Outcome));

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Mark? actualMark = sut.GetMark(row, column);
                    Mark? expectedMark = freshBoard.GetMark(row, column);
                    string message = $"Cell ({row}, {column}) should match a fresh board.";
                    Assert.That(actualMark, Is.EqualTo(expectedMark), message);
                }
            }
        }

        private static void PlayAll(BoardModel boardModel, (int Row, int Column)[] placements)
        {
            foreach ((int row, int column) in placements)
            {
                boardModel.PlaceMark(row, column);
            }
        }
    }
}
