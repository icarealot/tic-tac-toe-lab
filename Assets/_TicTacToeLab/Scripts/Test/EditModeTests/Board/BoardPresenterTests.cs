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
        private FakeLogService _fakeLogService;
        private BoardPresenter _boardPresenter;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            _fakeLogService = new FakeLogService();
            _boardPresenter = BoardPresenterBuilder.Build(
                _boardModel, _fakeBoardView, _fakeInputService, _fakeLogService);
        }

        [Test]
        public void A_press_on_an_empty_cell_shows_an_X_in_that_cell_and_the_model_records_it()
        {
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 2);

            Assert.That(_fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 2, Mark.X) }));
            Assert.That(_boardModel.GetMark(0, 2), Is.EqualTo(Mark.X));
        }

        [Test]
        public void Two_presses_on_empty_cells_show_an_X_and_then_an_O()
        {
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 1);

            Assert.That(_fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 0, Mark.X), (0, 1, Mark.O) }));
        }

        [Test]
        public void A_press_on_a_cell_already_holding_a_mark_leaves_the_turn_untouched_so_the_next_press_shows_the_mark_that_would_have_come_next_anyway()
        {
            BoardMoves.PressCell(_fakeInputService, _boardModel, 1, 1);
            BoardMoves.PressCell(_fakeInputService, _boardModel, 1, 1);
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 1);

            Assert.That(_fakeBoardView.ShownMarks, Is.EqualTo(new[] { (1, 1, Mark.X), (0, 1, Mark.O) }));
            Assert.That(_boardModel.GetMark(1, 1), Is.EqualTo(Mark.X));
            Assert.That(_fakeLogService.Messages, Is.EqualTo(new[] { "Rejected press on occupied cell (1, 1)" }));
        }

        [Test]
        public void A_press_resolving_to_no_cell_leaves_the_turn_untouched_so_the_next_press_shows_the_mark_that_would_have_come_next_anyway()
        {
            BoardMoves.PressCell(_fakeInputService, _boardModel, 1, 1);
            _fakeInputService.RaisePress(new Vector2(0.55f, 0f));
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 1);

            Assert.That(_fakeBoardView.ShownMarks, Is.EqualTo(new[] { (1, 1, Mark.X), (0, 1, Mark.O) }));
        }

        [Test]
        public void A_press_off_the_board_shows_nothing()
        {
            _fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(_fakeBoardView.ShownMarks, Is.Empty);
        }

        [Test]
        public void A_press_after_a_win_shows_nothing_and_places_nothing()
        {
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            _fakeBoardView.ShownMarks.Clear();
            BoardMoves.PressCell(_fakeInputService, _boardModel, 2, 2);

            Assert.That(_fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(_boardModel.IsEmpty(2, 2), Is.True);
        }

        [Test]
        public void A_press_after_a_win_is_logged_as_refused()
        {
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            _fakeLogService.Messages.Clear();
            BoardMoves.PressCell(_fakeInputService, _boardModel, 2, 2);

            Assert.That(_fakeLogService.Messages, Is.EqualTo(new[] { "Rejected press: the game is over" }));
        }

        [Test]
        public void A_win_is_announced_in_the_log_naming_the_winner()
        {
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(_fakeLogService.Messages, Is.EqualTo(new[] { "X wins" }));
        }

        [Test]
        public void A_drawn_game_is_announced_in_the_log_as_a_draw()
        {
            BoardMoves.PressToDraw(_fakeInputService, _boardModel);

            Assert.That(_fakeLogService.Messages, Is.EqualTo(new[] { "Draw" }));
        }

        [Test]
        public void Resetting_the_presenter_resets_the_model_and_clears_the_view()
        {
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            _boardPresenter.Reset();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void A_press_after_a_draw_shows_nothing_and_places_nothing_and_is_logged_as_refused()
        {
            BoardMoves.PressToDraw(_fakeInputService, _boardModel);

            _fakeBoardView.ShownMarks.Clear();
            _fakeLogService.Messages.Clear();
            _fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(_fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(_fakeLogService.Messages, Is.EqualTo(new[] { "Rejected press: the game is over" }));
        }

        private Func<int> SubscribeGameEndedCounter(BoardPresenter boardPresenter)
        {
            int count = 0;
            boardPresenter.GameEnded += () => count++;
            return () => count;
        }

        [Test]
        public void The_game_ended_event_is_raised_once_when_a_line_is_won()
        {
            Func<int> gameEndedCount = SubscribeGameEndedCounter(_boardPresenter);

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(gameEndedCount(), Is.EqualTo(1));
        }

        [Test]
        public void The_game_ended_event_is_raised_once_when_the_board_fills_with_no_line_won()
        {
            Func<int> gameEndedCount = SubscribeGameEndedCounter(_boardPresenter);

            BoardMoves.PressToDraw(_fakeInputService, _boardModel);

            Assert.That(gameEndedCount(), Is.EqualTo(1));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_placement_that_leaves_the_game_in_progress()
        {
            Func<int> gameEndedCount = SubscribeGameEndedCounter(_boardPresenter);

            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_press_on_an_occupied_cell()
        {
            BoardMoves.PressCell(_fakeInputService, _boardModel, 1, 1);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(_boardPresenter);

            BoardMoves.PressCell(_fakeInputService, _boardModel, 1, 1);

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_press_resolving_to_no_cell()
        {
            Func<int> gameEndedCount = SubscribeGameEndedCounter(_boardPresenter);

            _fakeInputService.RaisePress(new Vector2(0.55f, 0f));

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_press_after_the_game_is_already_over()
        {
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(_boardPresenter);

            BoardMoves.PressCell(_fakeInputService, _boardModel, 2, 2);

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_turn_changed_event_reports_the_new_turn_after_each_placed_mark()
        {
            List<Mark> turns = new();
            _boardPresenter.TurnChanged += turn => turns.Add(turn);

            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 1);

            Assert.That(turns, Is.EqualTo(new[] { Mark.O, Mark.X }));
        }

        [Test]
        public void The_turn_changed_event_is_not_raised_by_a_press_that_places_no_mark()
        {
            int turnChangedCount = 0;
            _boardPresenter.TurnChanged += _ => turnChangedCount++;

            _fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(turnChangedCount, Is.EqualTo(0));
        }

        [Test]
        public void A_subscriber_can_detach_from_the_game_ended_event()
        {
            int gameEndedCount = 0;
            Action handler = () => gameEndedCount++;
            _boardPresenter.GameEnded += handler;
            _boardPresenter.GameEnded -= handler;

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(0));
        }
    }
}
