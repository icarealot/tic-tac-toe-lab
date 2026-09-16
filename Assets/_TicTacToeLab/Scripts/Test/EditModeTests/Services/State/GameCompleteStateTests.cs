using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameCompleteStateTests
    {
        // --- Entering schedules the outcome presentation ---

        [Test]
        public void Entering_the_state_schedules_one_outcome_presentation_after_a_one_second_pause_without_immediately_showing_a_popup_resetting_the_board_or_changing_state()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            RecordingBoardSession boardSession = new(log) { Outcome = Outcome.Win, Turn = Mark.X };
            RecordingStateMachine stateMachine = new(log);
            GameCompleteState sut = new(boardSession, stateMachine, fakeUIService, fakeCoroutineService, new FakeInputService());

            // Act
            sut.Enter();

            // Assert
            Assert.That(fakeCoroutineService.HasScheduledCallback, Is.True);
            Assert.That(fakeCoroutineService.RunAfterCount, Is.EqualTo(1));
            Assert.That(fakeCoroutineService.ScheduledDelaySeconds, Is.EqualTo(1f));
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(0));
            Assert.That(fakeUIService.HasPopup, Is.False);
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void Entering_the_state_disables_player_presses()
        {
            // Arrange
            FakeInputService fakeInputService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(new List<string>()),
                new RecordingStateMachine(new List<string>()),
                new FakeUIService(),
                new FakeCoroutineService(),
                fakeInputService);

            // Act
            sut.Enter();

            // Assert
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.False);
        }

        [TestCase(Outcome.Win, Mark.X)]
        [TestCase(Outcome.Win, Mark.O)]
        [TestCase(Outcome.Draw, Mark.X)]
        public void The_scheduled_presentation_shows_one_outcome_popup_configured_with_the_captured_outcome_and_turn(Outcome outcome, Mark turn)
        {
            // Arrange
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            RecordingBoardSession boardSession = new(new List<string>()) { Outcome = outcome, Turn = turn };
            GameCompleteState sut = new(
                boardSession,
                new RecordingStateMachine(new List<string>()),
                fakeUIService,
                fakeCoroutineService,
                new FakeInputService());
            sut.Enter();

            // Act
            fakeCoroutineService.FireScheduledCallback();

            // Assert
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(fakeUIService.HasPopup, Is.True);
            Assert.That(fakeUIService.LastOutcomePopup.Outcome, Is.EqualTo(outcome));
            Assert.That(fakeUIService.LastOutcomePopup.Turn, Is.EqualTo(turn));
        }

        [Test]
        public void The_presentation_uses_the_outcome_and_turn_captured_on_entry()
        {
            // Arrange
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            RecordingBoardSession boardSession = new(new List<string>()) { Outcome = Outcome.Win, Turn = Mark.X };
            GameCompleteState sut = new(
                boardSession,
                new RecordingStateMachine(new List<string>()),
                fakeUIService,
                fakeCoroutineService,
                new FakeInputService());
            sut.Enter();

            // Act
            boardSession.Outcome = Outcome.Draw;
            boardSession.Turn = Mark.O;
            fakeCoroutineService.FireScheduledCallback();

            // Assert
            Assert.That(fakeUIService.LastOutcomePopup.Outcome, Is.EqualTo(Outcome.Win));
            Assert.That(fakeUIService.LastOutcomePopup.Turn, Is.EqualTo(Mark.X));
        }

        // --- Acknowledging the outcome with Continue or Back ---

        [Test]
        public void Back_during_the_pending_delay_has_no_effect()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(log),
                new RecordingStateMachine(log),
                fakeUIService,
                fakeCoroutineService,
                new FakeInputService());
            sut.Enter();

            // Act
            sut.Back();

            // Assert
            Assert.That(log, Is.Empty);
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(0));
            Assert.That(fakeCoroutineService.HasScheduledCallback, Is.True);
        }

        [Test]
        public void Continuing_the_outcome_popup_enters_the_main_menu_without_resetting_the_board_session()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(log),
                new RecordingStateMachine(log),
                fakeUIService,
                fakeCoroutineService,
                new FakeInputService());
            sut.Enter();
            fakeCoroutineService.FireScheduledCallback();

            // Act
            fakeUIService.LastOutcomePopup.Continue();

            // Assert
            Assert.That(log, Is.EqualTo(new[]
            {
                "StateMachine.ChangeState<MainMenuState>",
            }));
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
        }

        [Test]
        public void Back_after_the_popup_is_presented_enters_the_main_menu_without_resetting_the_board_session()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(log),
                new RecordingStateMachine(log),
                fakeUIService,
                fakeCoroutineService,
                new FakeInputService());
            sut.Enter();
            fakeCoroutineService.FireScheduledCallback();

            // Act
            sut.Back();

            // Assert
            Assert.That(log, Is.EqualTo(new[]
            {
                "StateMachine.ChangeState<MainMenuState>",
            }));
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
        }

        // --- Leaving restores input and cleans up its popup ---

        [Test]
        public void Leaving_before_the_presentation_cancels_it_and_restores_player_presses()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            FakeInputService fakeInputService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(log),
                new RecordingStateMachine(log),
                fakeUIService,
                fakeCoroutineService,
                fakeInputService);
            sut.Enter();

            // Act
            sut.Leave();
            fakeCoroutineService.FireScheduledCallback();

            // Assert
            Assert.That(log, Is.Empty);
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(0));
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Leaving_after_the_popup_is_presented_closes_it_and_restores_player_presses()
        {
            // Arrange
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            FakeInputService fakeInputService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(new List<string>()),
                new RecordingStateMachine(new List<string>()),
                fakeUIService,
                fakeCoroutineService,
                fakeInputService);
            sut.Enter();
            fakeCoroutineService.FireScheduledCallback();
            Assert.That(fakeUIService.HasPopup, Is.True);

            // Act
            sut.Leave();

            // Assert
            Assert.That(fakeUIService.HasPopup, Is.False);
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.True);
        }
    }
}
