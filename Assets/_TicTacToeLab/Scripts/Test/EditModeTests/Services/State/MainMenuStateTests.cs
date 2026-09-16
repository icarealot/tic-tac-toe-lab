using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class MainMenuStateTests
    {
        [Test]
        public void Starting_the_game_resets_the_board_session_before_entering_gameplay()
        {
            // Arrange
            List<string> log = new();
            FakeUIService fakeUIService = new();
            MainMenuState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), fakeUIService);

            sut.Enter();

            // Act
            fakeUIService.LastMainMenuPanel.StartGame();

            // Assert
            Assert.That(log, Is.EqualTo(new[]
            {
                "BoardSession.Reset",
                "StateMachine.ChangeState<GameplayState>",
            }));
        }

        [Test]
        public void Entering_the_menu_leaves_the_board_session_unreset()
        {
            // Arrange
            List<string> log = new();
            MainMenuState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), new FakeUIService());

            // Act
            sut.Enter();

            // Assert
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void Back_on_the_menu_changes_no_state_or_window()
        {
            // Arrange
            List<string> log = new();
            FakeUIService fakeUIService = new();
            MainMenuState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), fakeUIService);

            sut.Enter();

            // Act
            sut.Back();

            // Assert
            Assert.That(log, Is.Empty);
            Assert.That(fakeUIService.HasPanel, Is.True);
            Assert.That(fakeUIService.HasPopup, Is.False);
        }

        [Test]
        public void Leaving_the_menu_closes_its_panel()
        {
            // Arrange
            List<string> log = new();
            FakeUIService fakeUIService = new();
            MainMenuState sut = new(new RecordingBoardSession(log), new RecordingStateMachine(log), fakeUIService);

            sut.Enter();

            // Act
            sut.Leave();

            // Assert
            Assert.That(fakeUIService.HasPanel, Is.False);
        }
    }
}
