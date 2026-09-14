using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardSessionTests
    {
        private BoardModel _boardModel;
        private BoardPresser _presser;
        private BoardSession _boardSession;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _presser = new BoardPresser(fakeInputService, _boardModel);
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(_boardModel, fakeBoardView, fakeInputService);
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
    }
}
