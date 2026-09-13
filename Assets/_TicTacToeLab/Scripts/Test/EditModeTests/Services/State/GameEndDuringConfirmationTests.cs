using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameEndDuringConfirmationTests
    {
        private FakeBoardSession _fakeBoardSession;
        private FakeInputService _fakeInputService;
        private FakeUIService _fakeUIService;
        private FakeCoroutineService _fakeCoroutineService;
        private AppStateMachine _stateMachine;

        [SetUp]
        public void SetUp()
        {
            _fakeBoardSession = new FakeBoardSession();
            _fakeInputService = new FakeInputService();
            _fakeUIService = new FakeUIService();
            _fakeCoroutineService = new FakeCoroutineService();
            _stateMachine = new AppStateMachine(_fakeInputService);
            _stateMachine.Add(new GameplayState(_fakeBoardSession, _stateMachine, _fakeUIService, _fakeInputService));
            _stateMachine.Add(new GameCompleteState(_fakeBoardSession, _stateMachine, _fakeCoroutineService));
        }

        [TearDown]
        public void TearDown()
        {
            _stateMachine?.Dispose();
        }

        [Test]
        public void An_ending_while_the_confirmation_is_up_clears_it_and_restores_board_input_before_the_pause()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();
            Assert.That(_fakeUIService.HasPopup, Is.True);

            _fakeBoardSession.RaiseGameEnded();

            Assert.That(_fakeUIService.HasPopup, Is.False);
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_fakeCoroutineService.HasScheduledCallback, Is.True);
        }
    }
}
