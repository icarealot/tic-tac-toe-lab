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
        private BoardPresser _presser;
        private BoardSession _boardSession;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            _presser = new BoardPresser(_fakeInputService, _boardModel);
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(_boardModel, _fakeBoardView, _fakeInputService);
            _boardSession = new BoardSession(boardPresenter);
        }

        [Test]
        public void A_reset_requested_through_the_session_leaves_the_game_fresh()
        {
            BoardMoves.WinRowZeroForX(_presser);

            _boardSession.Reset();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void Disposing_the_session_stops_board_events_from_escaping_and_disposes_press_handling()
        {
            List<Mark> turnEvents = new();
            int gameEndedEvents = 0;
            _boardSession.TurnChanged += turn => turnEvents.Add(turn);
            _boardSession.GameEnded += () => gameEndedEvents++;

            BoardMoves.WinRowZeroForX(_presser);

            Assert.That(turnEvents, Is.Not.Empty);
            Assert.That(gameEndedEvents, Is.EqualTo(1));

            _boardSession.Dispose();

            turnEvents.Clear();
            BoardMoves.WinRowZeroForX(_presser);

            Assert.That(turnEvents, Is.Empty);
            Assert.That(gameEndedEvents, Is.EqualTo(1));
            Assert.That(_fakeInputService.HasSubscribers, Is.False);
        }
    }
}
