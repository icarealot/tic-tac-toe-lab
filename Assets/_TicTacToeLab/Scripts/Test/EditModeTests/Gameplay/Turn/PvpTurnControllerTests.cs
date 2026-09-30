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

        [Test]
        public void Pausing_before_entering_PvP_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);

            // Act
            sut.Pause();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Resuming_before_entering_PvP_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);

            // Act
            sut.Resume();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Entering_an_active_PvP_controller_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            inputService.DisablePlayerPress();

            // Act
            sut.Enter();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Entering_a_paused_PvP_controller_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Pause();
            inputService.EnablePlayerPress();

            // Act
            sut.Enter();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Pausing_an_already_paused_PvP_controller_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Pause();
            inputService.EnablePlayerPress();

            // Act
            sut.Pause();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Resuming_an_active_PvP_controller_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            inputService.DisablePlayerPress();

            // Act
            sut.Resume();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Exiting_an_already_exited_PvP_controller_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Exit();
            inputService.EnablePlayerPress();

            // Act
            sut.Exit();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Entering_after_exiting_PvP_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Exit();
            inputService.EnablePlayerPress();

            // Act
            sut.Enter();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Pausing_after_exiting_PvP_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Exit();
            inputService.EnablePlayerPress();

            // Act
            sut.Pause();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Resuming_after_exiting_PvP_is_a_no_op()
        {
            // Arrange
            FakeInputService inputService = new();
            PvpTurnController sut = new(inputService);
            sut.Enter();
            sut.Exit();
            inputService.EnablePlayerPress();

            // Act
            sut.Resume();

            // Assert
            Assert.That(inputService.IsPlayerPressEnabled, Is.True);
        }
    }
}
