using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    // These tests deliberately wire the real board and state graph. The graph, rather than any one
    // fake, is what makes the subscription coverage valuable.
    public sealed class GameplayStateWiringTests
    {
        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private FakeCoroutineService _fakeCoroutineService;
        private BoardSession _boardSession;
        private UIService _uiService;
        private AppStateMachine _stateMachine;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            BoardPresenter boardPresenter = new(
                _boardModel, _fakeBoardView,
                _fakeInputService, new FakeCameraService(), new FakeLogService());
            _boardSession = new BoardSession(boardPresenter);
            _fakeCoroutineService = new FakeCoroutineService();
            _uiService = new UIService(
                new FakeComponentFactoryService(), new FakeUIRoot(), _fakeCoroutineService);
            _stateMachine = new AppStateMachine(_fakeInputService);

            GameplayState gameplayState = new(
                _boardSession, _stateMachine, _uiService,
                _fakeInputService, new FakeLogService());
            GameCompleteState gameCompleteState = new(
                _boardSession, _stateMachine, _fakeCoroutineService);
            _stateMachine.Add(gameplayState);
            _stateMachine.Add(gameCompleteState);
        }

        [TearDown]
        public void TearDown()
        {
            _stateMachine?.Dispose();
            _boardSession?.Dispose();
        }

        [Test]
        public void Re_entering_gameplay_after_each_game_does_not_accumulate_subscriptions()
        {
            _stateMachine.ChangeState<GameplayState>();

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();

            // An extra, leftover subscription from a prior game would clear the board more than once per ending.
            Assert.That(_fakeBoardView.ClearCount, Is.EqualTo(3));
        }

        [Test]
        public void Many_games_in_a_row_leave_exactly_one_active_subscription()
        {
            _stateMachine.ChangeState<GameplayState>();

            for (int i = 0; i < 5; i++)
            {
                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.FireScheduledCallback();
            }

            _boardSession.Dispose();

            Assert.That(_fakeInputService.HasSubscribers, Is.False);
        }

        [Test]
        public void A_game_that_ends_while_the_popup_is_up_leaves_the_phase_with_its_popups_cleared()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            Assert.That(() =>
            {
                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.FireScheduledCallback();
            }, Throws.Nothing);

            Assert.That(_uiService.HasPopup, Is.False);
        }

        [Test]
        public void Tearing_down_the_machine_leaves_gameplay_so_a_subsequent_ending_reaches_nothing()
        {
            _stateMachine.ChangeState<GameplayState>();

            _stateMachine.Dispose();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(_fakeBoardView.WasCleared, Is.False);
            Assert.That(_boardModel.Outcome, Is.Not.EqualTo(Outcome.InProgress));
        }
    }
}
