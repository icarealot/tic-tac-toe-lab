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
        public void An_accepted_press_follows_the_conversion_chain_to_render_the_addressed_cell_and_raises_one_turn_change()
        {
            // Arrange
            BoardModel boardModel = new();
            BoardLayout boardLayout = new(boardModel.Dimension);
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, boardLayout, fakeBoardView, fakeInputService);

            List<Mark> turnEvents = new();
            List<int> shownMarkCountsWhenTurnChanged = new();
            int gameEndedEvents = 0;
            sut.TurnChanged += turn =>
            {
                turnEvents.Add(turn);
                shownMarkCountsWhenTurnChanged.Add(fakeBoardView.ShownMarks.Count);
            };
            sut.GameEnded += () => gameEndedEvents++;

            // Act
            fakeInputService.RaisePress(new Vector2(1.2f, 1.2f)); // The center of cell (0, 2).
            fakeInputService.RaisePress(new Vector2(-1.2f, 0f)); // The center of cell (1, 0).

            // Assert
            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (new CellCoordinate(0, 2), Mark.X), (new CellCoordinate(1, 0), Mark.O) }));
            Assert.That(boardModel.GetMark(new CellCoordinate(0, 2)), Is.EqualTo(Mark.X));
            Assert.That(boardModel.GetMark(new CellCoordinate(1, 0)), Is.EqualTo(Mark.O));
            Assert.That(turnEvents, Is.EqualTo(new[] { Mark.O, Mark.X }), "Each accepted nonterminal placement should announce exactly one turn change to the other mark.");
            Assert.That(shownMarkCountsWhenTurnChanged, Is.EqualTo(new[] { 1, 2 }), "The placed mark should be shown before the turn change is announced.");
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
                    presser => presser.Press(new CellCoordinate(0, 0)),
                    new Vector2(-1.2f, 1.2f))) // The center of occupied cell (0, 0).
                .SetName("A_press_on_an_occupied_cell_leaves_the_game_unchanged");

            yield return new TestCaseData(new RejectedPress(
                    presser => presser.Press(new CellCoordinate(0, 0)),
                    new Vector2(0.55f, 0f))) // Between cell columns, so it resolves to no cell.
                .SetName("A_press_resolving_to_no_cell_leaves_the_game_unchanged");

            yield return new TestCaseData(new RejectedPress(
                    BoardPresses.WinRowZeroForX,
                    new Vector2(1.2f, -1.2f))) // The center of the still-empty cell (2, 2).
                .SetName("A_press_after_the_game_is_completed_leaves_the_game_unchanged");
        }

        [TestCaseSource(nameof(RejectedPresses))]
        public void A_rejected_press_leaves_the_game_unchanged(RejectedPress rejected)
        {
            // Arrange
            BoardModel boardModel = new();
            BoardLayout boardLayout = new(boardModel.Dimension);
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, boardLayout, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardLayout);

            rejected.Setup(presser);

            List<(CellCoordinate Coordinate, Mark Mark)> shownMarks = new(fakeBoardView.ShownMarks);
            Mark?[,] marks = BoardState.CaptureMarks(boardModel);
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
            Assert.That(BoardState.CaptureMarks(boardModel), Is.EqualTo(marks), "The model should record no new mark.");
            Assert.That(boardModel.Turn, Is.EqualTo(turnBefore));
            Assert.That(boardModel.Outcome, Is.EqualTo(outcomeBefore));
            Assert.That(turnEvents, Is.Empty);
            Assert.That(gameEndedEvents, Is.EqualTo(0));
        }

        // --- Game-ended events ---

        public sealed class CompletedGame
        {
            public readonly Action<BoardPresser> Play;
            public readonly CellCoordinate TerminalCoordinate;
            public readonly Mark TerminalMark;
            public readonly Outcome Outcome;
            public readonly Mark[] ExpectedTurnChanges;

            public CompletedGame(
                Action<BoardPresser> play,
                CellCoordinate terminalCoordinate,
                Mark terminalMark,
                Outcome outcome,
                Mark[] expectedTurnChanges)
            {
                Play = play;
                TerminalCoordinate = terminalCoordinate;
                TerminalMark = terminalMark;
                Outcome = outcome;
                ExpectedTurnChanges = expectedTurnChanges;
            }
        }

        private static IEnumerable<TestCaseData> CompletedGames()
        {
            yield return new TestCaseData(new CompletedGame(
                    BoardPresses.WinRowZeroForX,
                    new CellCoordinate(0, 2),
                    Mark.X,
                    Outcome.XWin,
                    new[] { Mark.O, Mark.X, Mark.O, Mark.X }))
                .SetName("A_winning_placement_shows_its_mark_keeps_the_turn_and_raises_only_the_game_ended_event");

            yield return new TestCaseData(new CompletedGame(
                    BoardPresses.FillForDraw,
                    new CellCoordinate(2, 2),
                    Mark.X,
                    Outcome.Draw,
                    new[] { Mark.O, Mark.X, Mark.O, Mark.X, Mark.O, Mark.X, Mark.O, Mark.X }))
                .SetName("A_drawing_placement_shows_its_mark_keeps_the_turn_and_raises_only_the_game_ended_event");
        }

        [TestCaseSource(nameof(CompletedGames))]
        public void A_terminal_placement_shows_its_mark_keeps_the_turn_and_raises_only_the_game_ended_event(CompletedGame completedGame)
        {
            // Arrange
            BoardModel boardModel = new();
            BoardLayout boardLayout = new(boardModel.Dimension);
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, boardLayout, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardLayout);

            List<Mark> turnEvents = new();
            int gameEndedEvents = 0;
            int shownMarkCountWhenGameEnded = 0;
            sut.TurnChanged += turn => turnEvents.Add(turn);
            sut.GameEnded += () =>
            {
                gameEndedEvents++;
                shownMarkCountWhenGameEnded = fakeBoardView.ShownMarks.Count;
            };

            // Act
            completedGame.Play(presser);

            // Assert
            Assert.That(sut.Outcome, Is.EqualTo(completedGame.Outcome));
            Assert.That(sut.Turn, Is.EqualTo(completedGame.TerminalMark), "The turn should remain on the mark just placed.");
            Assert.That(turnEvents, Is.EqualTo(completedGame.ExpectedTurnChanges), "The terminal placement should not announce a turn change.");
            Assert.That(fakeBoardView.ShownMarks[^1], Is.EqualTo((completedGame.TerminalCoordinate, completedGame.TerminalMark)), "The terminal placement should be shown.");
            Assert.That(gameEndedEvents, Is.EqualTo(1));
            Assert.That(shownMarkCountWhenGameEnded, Is.EqualTo(fakeBoardView.ShownMarks.Count), "The terminal placement should be shown before the game-ended notification.");
        }

        // --- Reset and disposal ---

        [Test]
        public void Resetting_the_presenter_returns_the_model_to_a_fresh_state_and_clears_the_view()
        {
            // Arrange
            BoardModel boardModel = new();
            BoardLayout boardLayout = new(boardModel.Dimension);
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, boardLayout, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardLayout);

            BoardPresses.WinRowZeroForX(presser);

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
            BoardLayout boardLayout = new(boardModel.Dimension);
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresenter sut = BoardPresenterBuilder.Build(boardModel, boardLayout, fakeBoardView, fakeInputService);
            BoardPresser presser = new(fakeInputService, boardLayout);

            List<Mark> turnEvents = new();
            sut.TurnChanged += turn => turnEvents.Add(turn);

            // Act
            sut.Dispose();
            presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.True);
            Assert.That(turnEvents, Is.Empty);
        }
    }
}
