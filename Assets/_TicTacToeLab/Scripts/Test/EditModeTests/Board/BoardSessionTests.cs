using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardSessionTests
    {
        private BoardSession CreateSession(
            BoardModel boardModel,
            FakeBoardView boardView,
            FakeInputService inputService)
        {
            BoardPresenter boardPresenter = new(
                boardModel, boardView, factoryService: null,
                inputService, new FakeCameraService(), new FakeLogService());
            return new BoardSession(boardPresenter);
        }

        [Test]
        public void The_session_raises_its_ending_event_when_the_presenter_ends_the_game()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            int gameEndedCount = 0;
            boardSession.GameEnded += () => gameEndedCount++;

            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(1));
        }

        [Test]
        public void Resetting_the_game_through_the_session_reaches_the_presenter_and_clears_the_view()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            boardSession.Reset();

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void Disposing_the_session_unsubscribes_from_and_disposes_the_presenter_so_a_later_ending_raises_nothing()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            int gameEndedCount = 0;
            boardSession.GameEnded += () => gameEndedCount++;

            boardSession.Dispose();
            BoardMoves.WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(0));
            Assert.That(fakeInputService.HasSubscribers, Is.False);
        }

        [Test]
        public void The_session_reports_the_current_turn_and_raises_its_changed_event_as_marks_are_placed()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            List<Mark> turns = new();
            boardSession.TurnChanged += turn => turns.Add(turn);

            Assert.That(boardSession.Turn, Is.EqualTo(Mark.X));

            BoardMoves.PressCell(fakeInputService, boardModel, 0, 0);

            Assert.That(boardSession.Turn, Is.EqualTo(Mark.O));
            Assert.That(turns, Is.EqualTo(new[] { Mark.O }));
        }

        [Test]
        public void Disposing_the_session_stops_it_forwarding_turn_changes()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            int turnChangedCount = 0;
            boardSession.TurnChanged += _ => turnChangedCount++;

            boardSession.Dispose();
            BoardMoves.PressCell(fakeInputService, boardModel, 0, 0);

            Assert.That(turnChangedCount, Is.EqualTo(0));
        }
    }
}
