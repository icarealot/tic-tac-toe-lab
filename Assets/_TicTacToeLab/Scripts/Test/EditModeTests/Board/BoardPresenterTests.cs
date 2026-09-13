using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardPresenterTests
    {
        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private BoardPresser _presser;
        private BoardPresenter _boardPresenter;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            _boardPresenter = BoardPresenterBuilder.Build(_boardModel, _fakeBoardView, _fakeInputService);
            _presser = new BoardPresser(_fakeInputService, _boardModel);
        }

        // --- Accepted presses ---

        [Test]
        public void An_accepted_press_shows_the_current_mark_and_records_it_and_raises_one_turn_change()
        {
            List<Mark> turnEvents = new();
            int gameEndedEvents = 0;
            _boardPresenter.TurnChanged += turn => turnEvents.Add(turn);
            _boardPresenter.GameEnded += () => gameEndedEvents++;

            _presser.Press(0, 0);
            _presser.Press(1, 1);

            Assert.That(_fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 0, Mark.X), (1, 1, Mark.O) }));
            Assert.That(_boardModel.GetMark(0, 0), Is.EqualTo(Mark.X));
            Assert.That(_boardModel.GetMark(1, 1), Is.EqualTo(Mark.O));
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
            rejected.Setup(_presser);

            List<(int Row, int Column, Mark Mark)> shownMarks = new(_fakeBoardView.ShownMarks);
            Mark?[,] marks = CaptureMarks();
            Mark turnBefore = _boardModel.Turn;
            Outcome outcomeBefore = _boardModel.Outcome;
            List<Mark> turnEvents = new();
            int gameEndedEvents = 0;
            _boardPresenter.TurnChanged += turn => turnEvents.Add(turn);
            _boardPresenter.GameEnded += () => gameEndedEvents++;

            _fakeInputService.RaisePress(rejected.Point);

            Assert.That(_fakeBoardView.ShownMarks, Is.EqualTo(shownMarks), "The view should show no new mark.");
            Assert.That(CaptureMarks(), Is.EqualTo(marks), "The model should record no new mark.");
            Assert.That(_boardModel.Turn, Is.EqualTo(turnBefore));
            Assert.That(_boardModel.Outcome, Is.EqualTo(outcomeBefore));
            Assert.That(turnEvents, Is.Empty);
            Assert.That(gameEndedEvents, Is.EqualTo(0));
        }

        private Mark?[,] CaptureMarks()
        {
            Mark?[,] marks = new Mark?[_boardModel.Dimension, _boardModel.Dimension];

            for (int row = 0; row < _boardModel.Dimension; row++)
            {
                for (int column = 0; column < _boardModel.Dimension; column++)
                {
                    marks[row, column] = _boardModel.GetMark(row, column);
                }
            }

            return marks;
        }

        // --- Game-ended events ---

        private static readonly (int Row, int Column)[] _drawFillOrder = { (0, 0), (0, 1), (0, 2), (1, 1), (1, 0), (1, 2), (2, 1), (2, 0), (2, 2) };

        private static void PressToDraw(BoardPresser presser)
        {
            foreach ((int row, int column) in _drawFillOrder)
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
            int gameEndedEvents = 0;
            _boardPresenter.GameEnded += () => gameEndedEvents++;

            completeGame(_presser);

            Assert.That(gameEndedEvents, Is.EqualTo(1));
        }

        // --- Reset and disposal ---

        [Test]
        public void Resetting_the_presenter_returns_the_model_to_a_fresh_state_and_clears_the_view()
        {
            BoardMoves.WinRowZeroForX(_presser);

            _boardPresenter.Reset();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void Disposing_the_presenter_detaches_its_press_subscription()
        {
            List<Mark> turnEvents = new();
            _boardPresenter.TurnChanged += turn => turnEvents.Add(turn);

            _boardPresenter.Dispose();
            _presser.Press(0, 0);

            Assert.That(_fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(_boardModel.IsEmpty(0, 0), Is.True);
            Assert.That(turnEvents, Is.Empty);
        }
    }
}
