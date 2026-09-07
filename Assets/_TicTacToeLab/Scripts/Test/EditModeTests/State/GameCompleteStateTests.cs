using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameCompleteStateTests
    {
        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private BoardSession _boardSession;
        private FakeCoroutineService _fakeCoroutineService;
        private AppStateMachine _stateMachine;
        private GameCompleteState _gameCompleteState;

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
            _stateMachine = new AppStateMachine(_fakeInputService);
            UIService uiService = new(new FakeComponentFactoryService(), new FakeUIRoot(), new FakeCoroutineService());
            GameplayState gameplayState = new(_boardSession, _stateMachine, uiService, _fakeInputService, new FakeLogService());
            _gameCompleteState = new GameCompleteState(_boardSession, _stateMachine, _fakeCoroutineService);
            _stateMachine.Add(gameplayState);
            _stateMachine.Add(_gameCompleteState);
        }

        [Test]
        public void Entering_the_state_schedules_a_reset_after_one_second_without_resetting_yet()
        {
            _gameCompleteState.Enter();

            Assert.That(_fakeCoroutineService.HasScheduledCallback, Is.True);
            Assert.That(_fakeCoroutineService.ScheduledDelaySeconds, Is.EqualTo(1f));
            Assert.That(_fakeBoardView.WasCleared, Is.False);
        }

        [Test]
        public void Firing_the_scheduled_reset_resets_the_game_and_then_returns_to_gameplay()
        {
            _gameCompleteState.Enter();

            _fakeCoroutineService.FireScheduledCallback();

            Assert.That(_fakeBoardView.WasCleared, Is.True);
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));

            // Being back in gameplay is what lets a fresh press place a mark.
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
        }

        [Test]
        public void Leaving_the_state_disposes_the_pending_handle()
        {
            _gameCompleteState.Enter();

            _gameCompleteState.Leave();

            Assert.That(_fakeCoroutineService.WasStopped, Is.True);
        }

        [Test]
        public void A_disposed_pending_reset_does_not_later_reset_the_game()
        {
            _gameCompleteState.Enter();
            _gameCompleteState.Leave();

            _fakeCoroutineService.FireScheduledCallback();

            Assert.That(_fakeBoardView.WasCleared, Is.False);
        }
    }
}
