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

        // --- Game ending ---

        [Test]
        public void A_game_ending_enters_the_game_complete_state()
        {
            // Arrange
            FakeBoardSession fakeBoardSession = new();
            FakeInputService fakeInputService = new();
            FakeUIService fakeUIService = new();
            FakeStateMachine fakeStateMachine = new();
            GameplayState sut = new(fakeBoardSession, fakeStateMachine, fakeUIService, fakeInputService);

            sut.Enter();

            // Act
            fakeBoardSession.RaiseGameEnded();

            // Assert
            Assert.That(fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(GameCompleteState)));
        }

        [Test]
        public void Leaving_gameplay_stops_a_later_ending_from_reaching_the_state_machine()
        {
            // Arrange
            FakeBoardSession fakeBoardSession = new();
            FakeInputService fakeInputService = new();
            FakeUIService fakeUIService = new();
            FakeStateMachine fakeStateMachine = new();
            GameplayState sut = new(fakeBoardSession, fakeStateMachine, fakeUIService, fakeInputService);

            sut.Enter();
            sut.Leave();

            // Act
            fakeBoardSession.RaiseGameEnded();

            // Assert
            Assert.That(fakeStateMachine.ChangedStateType, Is.Null);
        }

        // --- Confirmation popup ---

        [Test]
        public void Back_with_no_confirmation_present_opens_one_popup_and_blocks_board_input()
        {
            // Arrange
            FakeBoardSession fakeBoardSession = new();
            FakeInputService fakeInputService = new();
            FakeUIService fakeUIService = new();
            FakeStateMachine fakeStateMachine = new();
            GameplayState sut = new(fakeBoardSession, fakeStateMachine, fakeUIService, fakeInputService);

            sut.Enter();

            // Act
            sut.Back();

            // Assert
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(fakeUIService.HasPopup, Is.True);
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.False);
            Assert.That(fakeStateMachine.ChangedStateType, Is.Null);
        }

        [TestCase(ConfirmationClosePath.BackAgain)]
        [TestCase(ConfirmationClosePath.AnswerNo)]
        public void Closing_the_confirmation_re_enables_board_input_and_opens_no_second_one(
            ConfirmationClosePath closePath)
        {
            // Arrange
            FakeBoardSession fakeBoardSession = new();
            FakeInputService fakeInputService = new();
            FakeUIService fakeUIService = new();
            FakeStateMachine fakeStateMachine = new();
            GameplayState sut = new(fakeBoardSession, fakeStateMachine, fakeUIService, fakeInputService);
            sut.Enter();
            sut.Back();

            // Act
            switch (closePath)
            {
                case ConfirmationClosePath.BackAgain:
                    sut.Back();
                    break;
                case ConfirmationClosePath.AnswerNo:
                    fakeUIService.LastPopup.No();
                    break;
            }

            // Assert
            Assert.That(fakeUIService.HasPopup, Is.False);
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.True);
            Assert.That(fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void Answering_yes_returns_to_the_menu()
        {
            // Arrange
            FakeBoardSession fakeBoardSession = new();
            FakeInputService fakeInputService = new();
            FakeUIService fakeUIService = new();
            FakeStateMachine fakeStateMachine = new();
            GameplayState sut = new(fakeBoardSession, fakeStateMachine, fakeUIService, fakeInputService);

            sut.Enter();
            sut.Back();

            // Act
            fakeUIService.LastPopup.Yes();

            // Assert
            Assert.That(fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(MainMenuState)));
        }

        [Test]
        public void Leaving_gameplay_with_a_confirmation_present_removes_it_closes_the_panel_and_restores_input()
        {
            // Arrange
            FakeBoardSession fakeBoardSession = new();
            FakeInputService fakeInputService = new();
            FakeUIService fakeUIService = new();
            FakeStateMachine fakeStateMachine = new();
            GameplayState sut = new(fakeBoardSession, fakeStateMachine, fakeUIService, fakeInputService);

            sut.Enter();
            sut.Back();

            // Act
            sut.Leave();

            // Arrange
            Assert.That(fakeUIService.HasPopup, Is.False);
            Assert.That(fakeUIService.HasPanel, Is.False);
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.True);
        }
    }
}
