using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class PvpTurnControllerTests
    {
        [Test]
        public void Entering_PvP_enables_board_presses()
        {
            // Arrange
            FakeInputService inputService = new();
            inputService.DisablePlayerPress();
            PvpTurnController sut = new(inputService);

            // Act
            sut.Enter();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Pausing_an_active_PvP_controller_disables_board_presses()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();

            // Act
            sut.Pause();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Resuming_a_paused_PvP_controller_enables_board_presses()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Pause();

            // Act
            sut.Resume();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Exiting_a_PvP_controller_disables_board_presses()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();

            // Act
            sut.Exit();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.False);
        }
    }
}
