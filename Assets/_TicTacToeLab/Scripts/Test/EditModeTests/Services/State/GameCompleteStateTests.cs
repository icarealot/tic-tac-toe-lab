using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameCompleteStateTests
    {
        private FakeBoardSession _fakeBoardSession;
        private FakeStateMachine _fakeStateMachine;
        private FakeCoroutineService _fakeCoroutineService;
        private GameCompleteState _gameCompleteState;

        [SetUp]
        public void SetUp()
        {
            _fakeBoardSession = new FakeBoardSession();
            _fakeStateMachine = new FakeStateMachine();
            _fakeCoroutineService = new FakeCoroutineService();
            _gameCompleteState = new GameCompleteState(
                _fakeBoardSession, _fakeStateMachine, _fakeCoroutineService);
        }

        [Test]
        public void Entering_the_state_schedules_a_reset_after_one_second_without_resetting_yet()
        {
            _gameCompleteState.Enter();

            Assert.That(_fakeCoroutineService.HasScheduledCallback, Is.True);
            Assert.That(_fakeCoroutineService.ScheduledDelaySeconds, Is.EqualTo(1f));
            Assert.That(_fakeBoardSession.WasReset, Is.False);
        }

        [Test]
        public void Firing_the_scheduled_reset_resets_the_game_and_then_returns_to_gameplay()
        {
            _gameCompleteState.Enter();

            _fakeCoroutineService.FireScheduledCallback();

            Assert.That(_fakeBoardSession.WasReset, Is.True);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(GameplayState)));
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

            Assert.That(_fakeBoardSession.WasReset, Is.False);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }
    }
}
