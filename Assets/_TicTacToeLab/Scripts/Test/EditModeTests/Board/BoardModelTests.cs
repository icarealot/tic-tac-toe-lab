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
        private static readonly (int Row, int Column)[] _winOnTheLastEmptyCell = { (0, 0), (2, 0), (0, 1), (1, 1), (1, 0), (1, 2), (2, 1), (2, 2), (0, 2) };

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

        // --- Starting state ---

        [Test]
        public void A_fresh_board_is_empty_with_X_to_play_and_in_progress()
        {
            BoardModel boardModel = new();

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Assert.That(boardModel.IsEmpty(row, column), Is.True, $"Cell ({row}, {column}) should be empty.");
                    Assert.That(boardModel.GetMark(row, column), Is.Null, $"Cell ({row}, {column}) should hold no mark.");
                }
            }

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        // --- Marks ---

        [Test]
        public void A_placement_records_the_current_turns_mark_only_in_the_addressed_cell()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(1, 1); // X
            boardModel.PlaceMark(0, 2); // O

            Mark?[,] expectedMarks =
            {
                { null, null, Mark.O },
                { null, Mark.X, null },
                { null, null, null },
            };

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Assert.That(boardModel.GetMark(row, column), Is.EqualTo(expectedMarks[row, column]),
                        $"Cell ({row}, {column}) should hold {expectedMarks[row, column]?.ToString() ?? "nothing"}.");
                }
            }
        }

        // --- Turns ---

        [TestCase(1, Mark.O)]
        [TestCase(2, Mark.X)]
        public void The_turn_advances_to_the_other_mark_after_each_placed_mark_while_the_game_is_in_progress(int placedMarks, Mark expectedTurn)
        {
            BoardModel boardModel = new();

            for (int index = 0; index < placedMarks; index++)
            {
                boardModel.PlaceMark(0, index);
            }

            Assert.That(boardModel.Turn, Is.EqualTo(expectedTurn));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [TestCaseSource(nameof(WinsByEachMark))]
        public void The_winning_placement_does_not_advance_the_turn_so_the_turn_names_the_winner(BoardScenario game)
        {
            BoardModel boardModel = new();
            PlayAll(boardModel, game.Placements);

            Assert.That(boardModel.Turn, Is.EqualTo(game.Winner));
        }

        [Test]
        public void The_final_drawing_placement_does_not_advance_the_turn()
        {
            BoardModel boardModel = new();
            PlayAll(boardModel, _fillsToDraw.Placements);

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X)); // X placed the final mark.
        }

        // --- Outcomes ---

        [TestCaseSource(nameof(WinningLines))]
        public void Completing_a_line_wins_the_game(BoardScenario game)
        {
            BoardModel boardModel = new();
            PlayAll(boardModel, game.Placements);

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        [TestCaseSource(nameof(NonWinningPatterns))]
        public void A_pattern_that_completes_no_line_leaves_the_game_in_progress(BoardScenario pattern)
        {
            BoardModel boardModel = new();
            PlayAll(boardModel, pattern.Placements);

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void A_board_filled_without_completing_a_line_is_a_draw()
        {
            BoardModel boardModel = new();
            PlayAll(boardModel, _fillsToDraw.Placements);

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Draw));
        }

        [Test]
        public void A_win_on_the_last_empty_cell_is_a_win_not_a_draw()
        {
            BoardModel boardModel = new();
            PlayAll(boardModel, _winOnTheLastEmptyCell);

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        // --- Reset ---

        [TestCaseSource(nameof(GamesToReset))]
        public void Resetting_a_played_board_leaves_the_same_public_state_as_a_fresh_board(BoardScenario game)
        {
            BoardModel playedBoard = new();
            PlayAll(playedBoard, game.Placements);
            BoardModel freshBoard = new();

            playedBoard.Reset();

            Assert.That(playedBoard.Turn, Is.EqualTo(freshBoard.Turn));
            Assert.That(playedBoard.Outcome, Is.EqualTo(freshBoard.Outcome));

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Assert.That(playedBoard.GetMark(row, column), Is.EqualTo(freshBoard.GetMark(row, column)),
                        $"Cell ({row}, {column}) should match a fresh board.");
                }
            }
        }

        private void PlayAll(BoardModel boardModel, (int Row, int Column)[] placements)
        {
            foreach ((int row, int column) in placements)
            {
                boardModel.PlaceMark(row, column);
            }
        }
    }
}
