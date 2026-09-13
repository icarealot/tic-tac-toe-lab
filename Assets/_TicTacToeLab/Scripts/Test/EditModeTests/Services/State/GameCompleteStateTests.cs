using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameCompleteStateTests
    {
        private List<string> _log;
        private FakeCoroutineService _fakeCoroutineService;
        private GameCompleteState _gameCompleteState;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _fakeCoroutineService = new FakeCoroutineService();
            _gameCompleteState = new GameCompleteState(new RecordingBoardSession(_log), new RecordingStateMachine(_log), _fakeCoroutineService);
        }

        [Test]
        public void Entering_the_state_schedules_one_reset_after_a_one_second_pause_without_resetting_immediately()
        {
            _gameCompleteState.Enter();

            Assert.That(_fakeCoroutineService.HasScheduledCallback, Is.True);
            Assert.That(_fakeCoroutineService.RunAfterCount, Is.EqualTo(1));
            Assert.That(_fakeCoroutineService.ScheduledDelaySeconds, Is.EqualTo(1f));
            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void The_delayed_reset_resets_the_board_session_before_returning_to_gameplay()
        {
            _gameCompleteState.Enter();

            _fakeCoroutineService.FireScheduledCallback();

            Assert.That(_log, Is.EqualTo(new[]
            {
                "BoardSession.Reset",
                "StateMachine.ChangeState<GameplayState>",
            }));
        }

        [Test]
        public void Leaving_the_state_cancels_the_pending_reset()
        {
            _gameCompleteState.Enter();

            _gameCompleteState.Leave();

            Assert.That(_fakeCoroutineService.WasStopped, Is.True);

            _fakeCoroutineService.FireScheduledCallback();

            Assert.That(_log, Is.Empty);
        }
    }
}
