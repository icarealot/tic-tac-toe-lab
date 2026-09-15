using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardPresenterTests
    {
        // --- Accepted presses ---

        [Test]
        public void An_accepted_press_shows_the_current_mark_and_records_it_and_raises_one_turn_change()
        {
            // Arrange
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardModel);

            List<Mark> turnEvents = new();
            int gameEndedEvents = 0;
            sut.TurnChanged += turn => turnEvents.Add(turn);
            sut.GameEnded += () => gameEndedEvents++;

            // Act
            presser.Press(0, 0);
            presser.Press(1, 1);

            // Assert
            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 0, Mark.X), (1, 1, Mark.O) }));
            Assert.That(boardModel.GetMark(0, 0), Is.EqualTo(Mark.X));
            Assert.That(boardModel.GetMark(1, 1), Is.EqualTo(Mark.O));
            Assert.That(turnEvents, Is.EqualTo(new[] { Mark.O, Mark.X }));
            Assert.That(gameEndedEvents, Is.EqualTo(0));
        }

        // --- Rejected presses ---

        public sealed class RejectedPress
        {
            public readonly Action<BoardPresser> Setup;
            public readonly Vector2 Point;

            public RejectedPress(Action<BoardPresser> setup, Vector2 point)
            {
                Setup = setup;
                Point = point;
            }
        }

        private static IEnumerable<TestCaseData> RejectedPresses()
        {
            yield return new TestCaseData(new RejectedPress(
                    presser => presser.Press(0, 0),
                    new Vector2(-1.2f, 1.2f))) // The center of occupied cell (0, 0).
                .SetName("A_press_on_an_occupied_cell_leaves_the_game_unchanged");

            yield return new TestCaseData(new RejectedPress(
                    presser => presser.Press(0, 0),
                    new Vector2(0.55f, 0f))) // Between cell columns, so it resolves to no cell.
                .SetName("A_press_resolving_to_no_cell_leaves_the_game_unchanged");

            yield return new TestCaseData(new RejectedPress(
                    BoardMoves.WinRowZeroForX,
                    new Vector2(1.2f, -1.2f))) // The center of the still-empty cell (2, 2).
                .SetName("A_press_after_the_game_is_completed_leaves_the_game_unchanged");
        }

        [TestCaseSource(nameof(RejectedPresses))]
        public void A_rejected_press_leaves_the_game_unchanged(RejectedPress rejected)
        {
            // Arrange
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardModel);

            rejected.Setup(presser);

            List<(int Row, int Column, Mark Mark)> shownMarks = new(fakeBoardView.ShownMarks);
            Mark?[,] marks = CaptureMarks(boardModel);
            Mark turnBefore = boardModel.Turn;
            Outcome outcomeBefore = boardModel.Outcome;
            List<Mark> turnEvents = new();
            int gameEndedEvents = 0;
            sut.TurnChanged += turn => turnEvents.Add(turn);
            sut.GameEnded += () => gameEndedEvents++;

            // Act
            fakeInputService.RaisePress(rejected.Point);

            // Assert
            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(shownMarks), "The view should show no new mark.");
            Assert.That(CaptureMarks(boardModel), Is.EqualTo(marks), "The model should record no new mark.");
            Assert.That(boardModel.Turn, Is.EqualTo(turnBefore));
            Assert.That(boardModel.Outcome, Is.EqualTo(outcomeBefore));
            Assert.That(turnEvents, Is.Empty);
            Assert.That(gameEndedEvents, Is.EqualTo(0));
        }

        // --- Game-ended events ---

        private static void PressToDraw(BoardPresser presser)
        {
            (int Row, int Column)[] drawFillOrder = { (0, 0), (0, 1), (0, 2), (1, 1), (1, 0), (1, 2), (2, 1), (2, 0), (2, 2) };
            foreach ((int row, int column) in drawFillOrder)
            {
                presser.Press(row, column);
            }
        }

        private static IEnumerable<TestCaseData> CompletedGames()
        {
            yield return new TestCaseData((Action<BoardPresser>)BoardMoves.WinRowZeroForX)
                .SetName("Completing_a_line_raises_the_game_ended_event_exactly_once");

            yield return new TestCaseData((Action<BoardPresser>)PressToDraw)
                .SetName("Filling_the_board_without_a_line_raises_the_game_ended_event_exactly_once");
        }

        [TestCaseSource(nameof(CompletedGames))]
        public void Completing_the_game_raises_the_game_ended_event_exactly_once(Action<BoardPresser> completeGame)
        {
            // Arrange
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardModel);

            int gameEndedEvents = 0;
            sut.GameEnded += () => gameEndedEvents++;

            // Act
            completeGame(presser);

            // Assert
            Assert.That(gameEndedEvents, Is.EqualTo(1));
        }

        // --- Reset and disposal ---

        [Test]
        public void Resetting_the_presenter_returns_the_model_to_a_fresh_state_and_clears_the_view()
        {
            // Arrange
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardModel);

            BoardMoves.WinRowZeroForX(presser);

            // Act
            sut.Reset();

            // Assert
            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void Disposing_the_presenter_detaches_its_press_subscription()
        {
            // Arrange
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardModel);

            List<Mark> turnEvents = new();
            sut.TurnChanged += turn => turnEvents.Add(turn);

            // Act
            sut.Dispose();
            presser.Press(0, 0);

            // Assert
            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(boardModel.IsEmpty(0, 0), Is.True);
            Assert.That(turnEvents, Is.Empty);
        }

        private static Mark?[,] CaptureMarks(BoardModel boardModel)
        {
            Mark?[,] marks = new Mark?[boardModel.Dimension, boardModel.Dimension];

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    marks[row, column] = boardModel.GetMark(row, column);
                }
            }

            return marks;
        }
    }
}
