using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardPresenterTests
    {
        private BoardPresenter CreatePresenter(
            BoardModel boardModel,
            FakeBoardView boardView,
            FakeInputService inputService,
            FakeLogService logService)
        {
            return new BoardPresenter(
                boardModel, boardView, factoryService: null,
                inputService, new FakeCameraService(), logService);
        }

        [Test]
        public void Initializing_the_presenter_delivers_exactly_nine_placements_one_per_cell_coordinate()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            _ = CreatePresenter(boardModel, fakeBoardView, new FakeInputService(), new FakeLogService());

            Assert.That(fakeBoardView.Placements, Has.Count.EqualTo(9));

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    bool found = false;
                    foreach (CellPlacement placement in fakeBoardView.Placements)
                    {
                        if (placement.Row != row || placement.Column != column)
                        {
                            continue;
                        }

                        found = true;
                        Assert.That(placement.LocalPoint, Is.EqualTo(boardModel.GetCellLocalPoint(row, column)));
                    }

                    Assert.That(found, Is.True, $"No placement was delivered for cell ({row}, {column}).");
                }
            }
        }

        [Test]
        public void A_press_on_an_empty_cell_shows_an_X_in_that_cell_and_the_model_records_it()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());

            BoardMoves.PressCell(fakeInputService, boardModel, 0, 2);

            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 2, Mark.X) }));
            Assert.That(boardModel.GetMark(0, 2), Is.EqualTo(Mark.X));
        }

        [Test]
        public void Two_presses_on_empty_cells_show_an_X_and_then_an_O()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());

            BoardMoves.PressCell(fakeInputService, boardModel, 0, 0);
            BoardMoves.PressCell(fakeInputService, boardModel, 0, 1);

            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 0, Mark.X), (0, 1, Mark.O) }));
        }

        [Test]
        public void A_press_on_a_cell_already_holding_a_mark_leaves_the_turn_untouched_so_the_next_press_shows_the_mark_that_would_have_come_next_anyway()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);

            BoardMoves.PressCell(fakeInputService, boardModel, 1, 1);
            BoardMoves.PressCell(fakeInputService, boardModel, 1, 1);
            BoardMoves.PressCell(fakeInputService, boardModel, 0, 1);

            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (1, 1, Mark.X), (0, 1, Mark.O) }));
            Assert.That(boardModel.GetMark(1, 1), Is.EqualTo(Mark.X));
            Assert.That(fakeLogService.Messages, Is.EqualTo(new[] { "Rejected press on occupied cell (1, 1)" }));
        }

        [Test]
        public void A_press_resolving_to_no_cell_leaves_the_turn_untouched_so_the_next_press_shows_the_mark_that_would_have_come_next_anyway()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());

            BoardMoves.PressCell(fakeInputService, boardModel, 1, 1);
            fakeInputService.RaisePress(new Vector2(0.55f, 0f));
            BoardMoves.PressCell(fakeInputService, boardModel, 0, 1);

            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (1, 1, Mark.X), (0, 1, Mark.O) }));
        }

        [Test]
        public void A_press_off_the_board_shows_nothing()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());

            fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
        }

        [Test]
        public void A_press_after_a_win_shows_nothing_and_places_nothing()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());
            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            fakeBoardView.ShownMarks.Clear();
            BoardMoves.PressCell(fakeInputService, boardModel, 2, 2);

            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(boardModel.IsEmpty(2, 2), Is.True);
        }

        [Test]
        public void A_press_after_a_win_is_logged_as_refused()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            fakeLogService.Messages.Clear();
            BoardMoves.PressCell(fakeInputService, boardModel, 2, 2);

            Assert.That(fakeLogService.Messages, Is.EqualTo(new[] { "Rejected press: the game is over" }));
        }

        [Test]
        public void A_win_is_announced_in_the_log_naming_the_winner()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);

            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(fakeLogService.Messages, Is.EqualTo(new[] { "X wins" }));
        }

        [Test]
        public void A_drawn_game_is_announced_in_the_log_as_a_draw()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);

            BoardMoves.PressToDraw(fakeInputService, boardModel);

            Assert.That(fakeLogService.Messages, Is.EqualTo(new[] { "Draw" }));
        }

        [Test]
        public void Resetting_the_presenter_resets_the_model_and_clears_the_view()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            boardPresenter.Reset();

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void A_press_after_a_draw_shows_nothing_and_places_nothing_and_is_logged_as_refused()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            BoardMoves.PressToDraw(fakeInputService, boardModel);

            fakeBoardView.ShownMarks.Clear();
            fakeLogService.Messages.Clear();
            fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(fakeLogService.Messages, Is.EqualTo(new[] { "Rejected press: the game is over" }));
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
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(boardPresenter);

            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(gameEndedCount(), Is.EqualTo(1));
        }

        [Test]
        public void The_game_ended_event_is_raised_once_when_the_board_fills_with_no_line_won()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(boardPresenter);

            BoardMoves.PressToDraw(fakeInputService, boardModel);

            Assert.That(gameEndedCount(), Is.EqualTo(1));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_placement_that_leaves_the_game_in_progress()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(boardPresenter);

            BoardMoves.PressCell(fakeInputService, boardModel, 0, 0);

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_press_on_an_occupied_cell()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            BoardMoves.PressCell(fakeInputService, boardModel, 1, 1);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(boardPresenter);

            BoardMoves.PressCell(fakeInputService, boardModel, 1, 1);

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_press_resolving_to_no_cell()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(boardPresenter);

            fakeInputService.RaisePress(new Vector2(0.55f, 0f));

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_game_ended_event_is_not_raised_by_a_press_after_the_game_is_already_over()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);
            Func<int> gameEndedCount = SubscribeGameEndedCounter(boardPresenter);

            BoardMoves.PressCell(fakeInputService, boardModel, 2, 2);

            Assert.That(gameEndedCount(), Is.EqualTo(0));
        }

        [Test]
        public void The_turn_changed_event_reports_the_new_turn_after_each_placed_mark()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            List<Mark> turns = new();
            boardPresenter.TurnChanged += turn => turns.Add(turn);

            BoardMoves.PressCell(fakeInputService, boardModel, 0, 0);
            BoardMoves.PressCell(fakeInputService, boardModel, 0, 1);

            Assert.That(turns, Is.EqualTo(new[] { Mark.O, Mark.X }));
        }

        [Test]
        public void The_turn_changed_event_is_not_raised_by_a_press_that_places_no_mark()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            int turnChangedCount = 0;
            boardPresenter.TurnChanged += _ => turnChangedCount++;

            fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(turnChangedCount, Is.EqualTo(0));
        }

        [Test]
        public void A_subscriber_can_detach_from_the_game_ended_event()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            FakeLogService fakeLogService = new();
            BoardPresenter boardPresenter = CreatePresenter(boardModel, fakeBoardView, fakeInputService, fakeLogService);
            int gameEndedCount = 0;
            Action handler = () => gameEndedCount++;
            boardPresenter.GameEnded += handler;
            boardPresenter.GameEnded -= handler;

            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(0));
        }
    }
}
