using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameCompleteStateTests
    {
        [Test]
        public void Entering_the_state_schedules_one_reset_after_a_one_second_pause_without_resetting_immediately()
        {
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            GameCompleteState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), fakeCoroutineService);

            sut.Enter();

            Assert.That(fakeCoroutineService.HasScheduledCallback, Is.True);
            Assert.That(fakeCoroutineService.RunAfterCount, Is.EqualTo(1));
            Assert.That(fakeCoroutineService.ScheduledDelaySeconds, Is.EqualTo(1f));
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void The_delayed_reset_resets_the_board_session_before_returning_to_gameplay()
        {
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            GameCompleteState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), fakeCoroutineService);

            sut.Enter();

            fakeCoroutineService.FireScheduledCallback();

            Assert.That(log, Is.EqualTo(new[]
            {
                "BoardSession.Reset",
                "StateMachine.ChangeState<GameplayState>",
            }));
        }

        [Test]
        public void Leaving_the_state_cancels_the_pending_reset()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            GameCompleteState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), fakeCoroutineService);

            sut.Enter();

            // Act
            sut.Leave();
            fakeCoroutineService.FireScheduledCallback();

            // Assert
            Assert.That(log, Is.Empty);
        }
    }
}
