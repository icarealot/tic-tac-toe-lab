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
        public void Board_presses_stay_disabled_during_the_delay_and_the_outcome_popup()
        {
            // Arrange
            FakeInputService fakeInputService = new();
            FakeCoroutineService fakeCoroutineService = new();
            GameCompleteState sut = new(
                new RecordingBoardSession(new List<string>()),
                new RecordingStateMachine(new List<string>()),
                new FakeUIService(),
                fakeCoroutineService,
                fakeInputService);

            // Act
            sut.Enter();

            // Assert
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.False);

            // Act
            fakeCoroutineService.FireScheduledCallback();

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
            Assert.That(fakeCoroutineService.WasStopped, Is.False);

            // Act
            fakeCoroutineService.FireScheduledCallback();

            // Assert
            // Back neither cancelled the delay nor showed the popup early.
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(log, Is.Empty);
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

        [Test]
        public void A_repeated_continue_causes_only_one_main_menu_transition_without_resetting_the_board_session()
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
            fakeUIService.LastOutcomePopup.Continue();

            // Assert
            Assert.That(log, Is.EqualTo(new[]
            {
                "StateMachine.ChangeState<MainMenuState>",
            }));
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
        }

        [Test]
        public void Back_acknowledgement_closes_the_popup_before_the_main_menu_panel_is_shown()
        {
            // Arrange
            // The real state machine performs the Leave-before-Enter sequence the ordering depends on,
            // and the fake UI service upholds the same no-panel-over-popup invariant as the real one.
            List<string> log = new();
            FakeInputService fakeInputService = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            RecordingBoardSession boardSession = new(log);
            AppStateMachine stateMachine = new(fakeInputService);
            stateMachine.Add(new MainMenuState(boardSession, stateMachine, fakeUIService));
            stateMachine.Add(new GameCompleteState(boardSession, stateMachine, fakeUIService, fakeCoroutineService, fakeInputService));
            stateMachine.ChangeState<GameCompleteState>();
            fakeCoroutineService.FireScheduledCallback();
            Assert.That(fakeUIService.HasPopup, Is.True);

            // Act
            fakeInputService.RaiseBack();

            // Assert
            Assert.That(fakeUIService.HasPopup, Is.False);
            Assert.That(fakeUIService.LastMainMenuPanel, Is.Not.Null);
            Assert.That(fakeInputService.IsPlayerPressEnabled, Is.True);
            Assert.That(log, Is.Empty);
        }

        // --- The outcome is presented and acknowledged only once ---

        [Test]
        public void A_repeated_outcome_presentation_shows_no_second_popup_or_transition()
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
            fakeCoroutineService.ReplayLastCallback();

            // Assert
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(log, Is.Empty);
        }

        [Test]
        public void A_late_outcome_presentation_after_acknowledgement_shows_no_second_popup_or_second_transition()
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
            fakeUIService.LastOutcomePopup.Continue();

            // Act
            fakeCoroutineService.ReplayLastCallback();

            // Assert
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(1));
            Assert.That(log, Is.EqualTo(new[]
            {
                "StateMachine.ChangeState<MainMenuState>",
            }));
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

        // --- The state repeats for a second completed game ---

        [Test]
        public void A_second_completed_game_presents_and_acknowledges_its_outcome_again()
        {
            // Arrange
            List<string> log = new();
            FakeCoroutineService fakeCoroutineService = new();
            FakeUIService fakeUIService = new();
            RecordingBoardSession boardSession = new(log) { Outcome = Outcome.Win, Turn = Mark.X };
            GameCompleteState sut = new(
                boardSession,
                new RecordingStateMachine(log),
                fakeUIService,
                fakeCoroutineService,
                new FakeInputService());

            sut.Enter();
            fakeCoroutineService.FireScheduledCallback();
            fakeUIService.LastOutcomePopup.Continue();
            sut.Leave();

            // Act
            sut.Enter();
            fakeCoroutineService.FireScheduledCallback();
            fakeUIService.LastOutcomePopup.Continue();

            // Assert
            Assert.That(fakeUIService.ShowPopupCount, Is.EqualTo(2));
            Assert.That(log, Is.EqualTo(new[]
            {
                "StateMachine.ChangeState<MainMenuState>",
                "StateMachine.ChangeState<MainMenuState>",
            }));
        }
    }
}
