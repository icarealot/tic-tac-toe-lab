using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardModelTests
    {
        public sealed class BoardScenario
        {
            public readonly CellCoordinate[] Placements;
            public readonly Outcome ExpectedOutcome;

            public BoardScenario(CellCoordinate[] placements, Outcome expectedOutcome = Outcome.InProgress)
            {
                Placements = placements;
                ExpectedOutcome = expectedOutcome;
            }
        }

        private static readonly BoardScenario _xWinsRowZero = new(new CellCoordinate[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(0, 2) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsRowOne = new(new CellCoordinate[] { new(1, 0), new(0, 0), new(1, 1), new(0, 1), new(1, 2) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsRowTwo = new(new CellCoordinate[] { new(2, 0), new(0, 0), new(2, 1), new(0, 1), new(2, 2) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsColumnZero = new(new CellCoordinate[] { new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(2, 0) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsColumnOne = new(new CellCoordinate[] { new(0, 1), new(0, 0), new(1, 1), new(1, 0), new(2, 1) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsColumnTwo = new(new CellCoordinate[] { new(0, 2), new(0, 0), new(1, 2), new(1, 0), new(2, 2) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsTheMainDiagonal = new(new CellCoordinate[] { new(0, 0), new(0, 1), new(1, 1), new(0, 2), new(2, 2) }, Outcome.XWin);
        private static readonly BoardScenario _xWinsTheAntiDiagonal = new(new CellCoordinate[] { new(0, 2), new(0, 0), new(1, 1), new(0, 1), new(2, 0) }, Outcome.XWin);
        private static readonly BoardScenario _oWinsRowTwo = new(new CellCoordinate[] { new(0, 0), new(2, 0), new(0, 1), new(2, 1), new(1, 0), new(2, 2) }, Outcome.OWin);
        private static readonly BoardScenario _leavesRowZeroIncomplete = new(new CellCoordinate[] { new(0, 0), new(1, 0), new(0, 1) });
        private static readonly BoardScenario _leavesRowZeroMixed = new(new CellCoordinate[] { new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(2, 1), new(0, 2) });
        private static readonly BoardScenario _fillsToDraw = new(new CellCoordinate[] { new(0, 0), new(0, 1), new(0, 2), new(1, 1), new(1, 0), new(1, 2), new(2, 1), new(2, 0), new(2, 2) }, Outcome.Draw);

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

        private static IEnumerable<BoardScenario> TerminalGames()
        {
            yield return _xWinsRowZero;
            yield return _oWinsRowTwo;
            yield return _fillsToDraw;
        }

        private static IEnumerable<TestCaseData> TerminalRejections()
        {
            yield return new TestCaseData(_xWinsRowZero, new CellCoordinate(2, 2))
                .SetName("A_placement_after_an_X_win_is_rejected_without_changing_the_board");

            yield return new TestCaseData(_oWinsRowTwo, new CellCoordinate(0, 2))
                .SetName("A_placement_after_an_O_win_is_rejected_without_changing_the_board");

            yield return new TestCaseData(_fillsToDraw, new CellCoordinate(0, 0))
                .SetName("A_placement_after_a_draw_is_rejected_without_changing_the_board");
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

        // --- Fresh board ---

        [Test]
        public void A_fresh_board_is_empty_with_X_to_play_and_the_outcome_in_progress()
        {
            BoardModel sut = new();

            Assert.That(sut.Turn, Is.EqualTo(Mark.X));
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(BoardState.CaptureMarks(sut).Cast<Mark?>(), Is.All.Null);
        }

        // --- Marks ---

        [Test]
        public void An_accepted_placement_returns_true_and_records_the_current_turns_mark_only_in_the_addressed_cell()
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
            bool firstAccepted = sut.TryPlaceMark(new CellCoordinate(1, 1)); // X
            bool secondAccepted = sut.TryPlaceMark(new CellCoordinate(0, 2)); // O

            // Assert
            Assert.That(firstAccepted, Is.True, "The placement (1, 1) should be accepted.");
            Assert.That(secondAccepted, Is.True, "The placement (0, 2) should be accepted.");
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Mark? actualMark = sut.GetMark(new CellCoordinate(row, column));
                    Mark? expectedMark = expectedMarks[row, column];
                    string message = $"Cell ({row}, {column}) should hold {expectedMarks[row, column]?.ToString() ?? "nothing"}.";
                    Assert.That(actualMark, Is.EqualTo(expectedMark), message);
                }
            }
        }

        // --- Rejected placements ---

        [TestCase(3, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 3)]
        [TestCase(0, -1)]
        public void An_out_of_range_coordinate_is_rejected_without_changing_the_board(int row, int column)
        {
            // Arrange
            BoardModel sut = new();
            Assert.That(sut.TryPlaceMark(new CellCoordinate(1, 1)), Is.True, "The setup mark (1, 1) should be accepted.");
            Mark turnBefore = sut.Turn;
            Outcome outcomeBefore = sut.Outcome;
            Mark?[,] marksBefore = BoardState.CaptureMarks(sut);

            // Act
            bool accepted = sut.TryPlaceMark(new CellCoordinate(row, column));

            // Assert
            Assert.That(accepted, Is.False);
            Assert.That(sut.Turn, Is.EqualTo(turnBefore));
            Assert.That(sut.Outcome, Is.EqualTo(outcomeBefore));
            Assert.That(BoardState.CaptureMarks(sut), Is.EqualTo(marksBefore), "No cell should change when the coordinate is outside the board.");
        }

        [Test]
        public void A_placement_on_a_marked_cell_is_rejected_without_changing_the_board()
        {
            // Arrange
            BoardModel sut = new();
            Assert.That(sut.TryPlaceMark(new CellCoordinate(0, 0)), Is.True, "The setup mark (0, 0) should be accepted.");
            Mark turnBefore = sut.Turn;
            Outcome outcomeBefore = sut.Outcome;
            Mark?[,] marksBefore = BoardState.CaptureMarks(sut);

            // Act
            bool accepted = sut.TryPlaceMark(new CellCoordinate(0, 0));

            // Assert
            Assert.That(accepted, Is.False);
            Assert.That(sut.Turn, Is.EqualTo(turnBefore));
            Assert.That(sut.Outcome, Is.EqualTo(outcomeBefore));
            Assert.That(BoardState.CaptureMarks(sut), Is.EqualTo(marksBefore), "A marked cell should keep its mark and the game should not advance.");
        }

        [TestCaseSource(nameof(TerminalRejections))]
        public void A_placement_after_a_terminal_outcome_is_rejected_without_changing_the_board(BoardScenario game, CellCoordinate coordinate)
        {
            // Arrange
            BoardModel sut = new();
            PlayAll(sut, game.Placements);
            Mark turnBefore = sut.Turn;
            Outcome outcomeBefore = sut.Outcome;
            Mark?[,] marksBefore = BoardState.CaptureMarks(sut);

            // Act
            bool accepted = sut.TryPlaceMark(coordinate);

            // Assert
            Assert.That(accepted, Is.False);
            Assert.That(sut.Turn, Is.EqualTo(turnBefore));
            Assert.That(sut.Outcome, Is.EqualTo(outcomeBefore));
            Assert.That(BoardState.CaptureMarks(sut), Is.EqualTo(marksBefore), "A completed game should accept no more marks.");
        }

        // --- Turns ---

        [TestCase(1, Mark.O)]
        [TestCase(2, Mark.X)]
        public void The_turn_advances_to_the_other_mark_after_each_accepted_placement_while_the_game_is_in_progress(int placedMarks, Mark expectedTurn)
        {
            BoardModel sut = new();

            for (int index = 0; index < placedMarks; index++)
            {
                CellCoordinate coordinate = new(0, index);
                bool accepted = sut.TryPlaceMark(coordinate);
                Assert.That(accepted, Is.True, $"The scenario should accept the placement ({coordinate.Row}, {coordinate.Column}).");
            }

            Assert.That(sut.Turn, Is.EqualTo(expectedTurn));
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [TestCaseSource(nameof(TerminalGames))]
        public void A_terminal_placement_leaves_the_turn_on_the_mark_that_just_played(BoardScenario game)
        {
            BoardModel sut = new();

            PlayAll(sut, game.Placements);

            Mark justPlayed = game.Placements.Length % 2 == 1 ? Mark.X : Mark.O;
            Assert.That(sut.Turn, Is.EqualTo(justPlayed));
        }

        // --- Outcomes ---

        [TestCaseSource(nameof(WinningLines))]
        public void Completing_a_line_reports_the_win_of_the_mark_that_completed_it(BoardScenario game)
        {
            BoardModel sut = new();

            PlayAll(sut, game.Placements);

            Assert.That(sut.Outcome, Is.EqualTo(game.ExpectedOutcome));
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
            CellCoordinate[] winOnTheLastEmptyCell = { new(0, 0), new(2, 0), new(0, 1), new(1, 1), new(1, 0), new(1, 2), new(2, 1), new(2, 2), new(0, 2) };
            BoardModel sut = new();

            PlayAll(sut, winOnTheLastEmptyCell);

            Assert.That(sut.Outcome, Is.EqualTo(Outcome.XWin));
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
                    Mark? actualMark = sut.GetMark(new CellCoordinate(row, column));
                    Mark? expectedMark = freshBoard.GetMark(new CellCoordinate(row, column));
                    string message = $"Cell ({row}, {column}) should match a fresh board.";
                    Assert.That(actualMark, Is.EqualTo(expectedMark), message);
                }
            }
        }

        private static void PlayAll(BoardModel boardModel, CellCoordinate[] placements)
        {
            foreach (CellCoordinate coordinate in placements)
            {
                bool accepted = boardModel.TryPlaceMark(coordinate);
                Assert.That(accepted, Is.True, $"The scenario should accept the placement ({coordinate.Row}, {coordinate.Column}).");
            }
        }
    }
}
