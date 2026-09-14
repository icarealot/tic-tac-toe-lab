using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameplayStateTests
    {
        public enum ConfirmationClosePath
        {
            BackAgain,
            AnswerNo,
        }

        private sealed class FakeStateMachine : IStateMachine
        {
            public Type ChangedStateType { get; private set; }

            public void ChangeState<TState>() where TState : IAppState
            {
                ChangedStateType = typeof(TState);
            }
        }

        private FakeBoardSession _fakeBoardSession;
        private FakeInputService _fakeInputService;
        private FakeUIService _fakeUIService;
        private FakeStateMachine _fakeStateMachine;
        private GameplayState _gameplayState;

        [SetUp]
        public void SetUp()
        {
            _fakeBoardSession = new FakeBoardSession();
            _fakeInputService = new FakeInputService();
            _fakeUIService = new FakeUIService();
            _fakeStateMachine = new FakeStateMachine();
            _gameplayState = new GameplayState(_fakeBoardSession, _fakeStateMachine, _fakeUIService, _fakeInputService);
        }

        [Test]
        public void A_game_ending_enters_the_game_complete_state()
        {
            _gameplayState.Enter();

            _fakeBoardSession.RaiseGameEnded();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(GameCompleteState)));
        }

        [Test]
        public void Leaving_gameplay_stops_a_later_ending_from_reaching_the_state_machine()
        {
            _gameplayState.Enter();
            _gameplayState.Leave();

            _fakeBoardSession.RaiseGameEnded();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void Back_with_no_confirmation_present_opens_one_popup_and_blocks_board_input()
        {
            _gameplayState.Enter();

            _gameplayState.Back();

            Assert.That(_fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(_fakeUIService.HasPopup, Is.True);
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [TestCase(ConfirmationClosePath.BackAgain)]
        [TestCase(ConfirmationClosePath.AnswerNo)]
        public void Closing_the_confirmation_re_enables_board_input_and_opens_no_second_one(
            ConfirmationClosePath closePath)
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            switch (closePath)
            {
                case ConfirmationClosePath.BackAgain:
                    _gameplayState.Back();
                    break;
                case ConfirmationClosePath.AnswerNo:
                    _fakeUIService.LastPopup.No();
                    break;
            }

            Assert.That(_fakeUIService.HasPopup, Is.False);
            Assert.That(_fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void Answering_yes_returns_to_the_menu()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            _fakeUIService.LastPopup.Yes();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(MainMenuState)));
        }

        [Test]
        public void Leaving_gameplay_with_a_confirmation_present_removes_it_closes_the_panel_and_restores_input()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            _gameplayState.Leave();

            Assert.That(_fakeUIService.HasPopup, Is.False);
            Assert.That(_fakeUIService.HasPanel, Is.False);
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }
    }
}
