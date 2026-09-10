using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardSessionTests
    {
        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private FakeLogService _fakeLogService;
        private BoardSession _boardSession;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            _fakeLogService = new FakeLogService();
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(
                _boardModel, _fakeBoardView, _fakeInputService, _fakeLogService);
            _boardSession = new BoardSession(boardPresenter);
        }

        [Test]
        public void The_session_raises_its_ending_event_when_the_presenter_ends_the_game()
        {
            int gameEndedCount = 0;
            _boardSession.GameEnded += () => gameEndedCount++;

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(1));
        }

        [Test]
        public void Resetting_the_game_through_the_session_reaches_the_presenter_and_clears_the_view()
        {
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            _boardSession.Reset();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void Disposing_the_session_unsubscribes_from_and_disposes_the_presenter_so_a_later_ending_raises_nothing()
        {
            int gameEndedCount = 0;
            _boardSession.GameEnded += () => gameEndedCount++;

            _boardSession.Dispose();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(0));
            Assert.That(_fakeInputService.HasSubscribers, Is.False);
        }

        [Test]
        public void The_session_reports_the_current_turn_and_raises_its_changed_event_as_marks_are_placed()
        {
            List<Mark> turns = new();
            _boardSession.TurnChanged += turn => turns.Add(turn);

            Assert.That(_boardSession.Turn, Is.EqualTo(Mark.X));

            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);

            Assert.That(_boardSession.Turn, Is.EqualTo(Mark.O));
            Assert.That(turns, Is.EqualTo(new[] { Mark.O }));
        }

        [Test]
        public void Disposing_the_session_stops_it_forwarding_turn_changes()
        {
            int turnChangedCount = 0;
            _boardSession.TurnChanged += _ => turnChangedCount++;

            _boardSession.Dispose();
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);

            Assert.That(turnChangedCount, Is.EqualTo(0));
        }
    }
}
