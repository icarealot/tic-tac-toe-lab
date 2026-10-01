using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class AppStateMachineTests
    {
        private BoardModel _boardModel;
        private BoardLayout _boardLayout;
        private BoardPresenter _boardPresenter;
        private FakeBoardView _boardView;
        private FakeInputService _inputService;
        private FakeAppUI _appUI;
        private FakeDelayScheduler _delayScheduler;
        private FakeRandomService _randomService;
        private BoardPresser _presser;
        private AppStateMachine _sut;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _boardLayout = new BoardLayout(_boardModel.Dimension);
            _boardView = new FakeBoardView();
            _inputService = new FakeInputService();
            _appUI = new FakeAppUI();
            _delayScheduler = new FakeDelayScheduler();
            _randomService = new FakeRandomService();

            _boardPresenter = BoardPresenterBuilder.Build(_boardModel, _boardLayout, _boardView, _inputService);
            _presser = new BoardPresser(_inputService, _boardLayout);
            _sut = new AppStateMachine(_boardPresenter, _appUI, _delayScheduler, _inputService, _randomService);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        private void StartGameplay()
        {
            _sut.Start();
            _appUI.HomeScreen.ClickPvp();
        }

        private void StartBotSelection()
        {
            _sut.Start();
            _appUI.HomeScreen.ClickPve();
        }

        private void StartAmateurPveGameplay()
        {
            StartBotSelection();
            _appUI.BotSelectionScreen.ClickAmateur();
        }

        private void StartProfessionalPveGameplay()
        {
            StartBotSelection();
            _appUI.BotSelectionScreen.ClickProfessional();
        }

        private void AssertLastRequested<TScreen>() where TScreen : IScreen
        {
            Assert.That(_appUI.LastRequestedRole, Is.EqualTo(typeof(TScreen)));
        }

        private void AssertLastClosed<TScreen>() where TScreen : IScreen
        {
            Assert.That(_appUI.LastClosedRole, Is.EqualTo(typeof(TScreen)));
        }

        // --- Startup and Home ---

        [Test]
        public void Starting_the_state_machine_shows_Home_with_board_presses_disabled()
        {
            // Act
            _sut.Start();

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Back_at_Home_has_no_effect()
        {
            // Arrange
            _sut.Start();
            int homePresentationsBefore = _appUI.HomeScreen.PresentationCount;

            // Act
            _inputService.RaiseBack();

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentationsBefore));
        }

        // --- Starting a game ---

        [Test]
        public void Starting_a_game_resets_the_existing_board_before_showing_gameplay_and_entering_the_controller()
        {
            // Arrange
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 2));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.XWin));
            _sut.Start();

            // Act
            _appUI.HomeScreen.ClickPvp();

            // Assert
            Assert.That(BoardState.IsEmpty(_boardModel), Is.True);
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_appUI.GameplayScreen.BoardOutcomeWhenPresented, Is.EqualTo(Outcome.InProgress));
            AssertLastRequested<IGameplayScreen>();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Starting_a_PvE_game_resets_to_an_empty_board_with_X_to_move_and_an_in_progress_outcome()
        {
            // Arrange
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 0));
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));

            // Act
            StartProfessionalPveGameplay();

            // Assert
            Assert.That(BoardState.IsEmpty(_boardModel), Is.True);
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_appUI.GameplayScreen.BoardOutcomeWhenPresented, Is.EqualTo(Outcome.InProgress));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        // --- Home choices and bot selection ---

        [Test]
        public void Starting_the_state_machine_offers_both_PvP_and_PvE_at_Home()
        {
            // Act
            _sut.Start();

            // Assert
            Assert.That(_appUI.HomeScreen.OnPvp, Is.Not.Null, "Home should offer the PvP choice.");
            Assert.That(_appUI.HomeScreen.OnPve, Is.Not.Null, "Home should offer the PvE choice.");
        }

        [Test]
        public void Selecting_PvP_starts_gameplay_with_the_PvP_setup()
        {
            // Arrange
            _sut.Start();

            // Act
            _appUI.HomeScreen.ClickPvp();

            // Assert
            AssertLastRequested<IGameplayScreen>();
            Assert.That(_appUI.BotSelectionScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.GameplayScreen.ShownSetup.Mode, Is.EqualTo(GameMode.Pvp));
            Assert.That(_appUI.GameplayScreen.ShownSetup.BotDifficulty, Is.Null);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void PvP_uses_alternating_human_placements_without_bot_work()
        {
            // Arrange
            StartGameplay();

            // Act
            _presser.Press(new CellCoordinate(0, 0));
            _presser.Press(new CellCoordinate(0, 1));

            // Assert
            Assert.That(_boardView.ShownMarks, Is.EqualTo(new[]
            {
                (new CellCoordinate(0, 0), Mark.X),
                (new CellCoordinate(0, 1), Mark.O),
            }));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_randomService.IntegerCallCount, Is.EqualTo(0));
            Assert.That(_randomService.FloatingPointCallCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Selecting_PvE_opens_bot_selection_instead_of_gameplay()
        {
            // Arrange
            _sut.Start();

            // Act
            _appUI.HomeScreen.ClickPve();

            // Assert
            AssertLastRequested<IBotSelectionScreen>();
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Selecting_an_amateur_bot_starts_gameplay_with_the_amateur_PvE_setup()
        {
            // Arrange
            StartBotSelection();

            // Act
            _appUI.BotSelectionScreen.ClickAmateur();

            // Assert
            AssertLastRequested<IGameplayScreen>();
            Assert.That(_appUI.BotSelectionScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.GameplayScreen.ShownSetup.Mode, Is.EqualTo(GameMode.Pve));
            Assert.That(_appUI.GameplayScreen.ShownSetup.BotDifficulty, Is.EqualTo(BotDifficulty.Amateur));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Selecting_a_professional_bot_starts_gameplay_with_the_professional_PvE_setup()
        {
            // Arrange
            StartBotSelection();

            // Act
            _appUI.BotSelectionScreen.ClickProfessional();

            // Assert
            AssertLastRequested<IGameplayScreen>();
            Assert.That(_appUI.BotSelectionScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.GameplayScreen.ShownSetup.Mode, Is.EqualTo(GameMode.Pve));
            Assert.That(_appUI.GameplayScreen.ShownSetup.BotDifficulty, Is.EqualTo(BotDifficulty.Professional));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        private static IEnumerable<TestCaseData> BotSelectionBackRequests()
        {
            yield return new TestCaseData((Action<FakeAppUI, FakeInputService>)((appUI, _) => appUI.BotSelectionScreen.ClickBack()))
                .SetName("Clicking_Back_at_bot_selection_returns_Home_without_confirmation");

            yield return new TestCaseData((Action<FakeAppUI, FakeInputService>)((_, inputService) => inputService.RaiseBack()))
                .SetName("Raising_application_Back_at_bot_selection_returns_Home_without_confirmation");
        }

        [TestCaseSource(nameof(BotSelectionBackRequests))]
        public void Back_at_bot_selection_returns_Home_without_confirmation(
            Action<FakeAppUI, FakeInputService> requestBack)
        {
            // Arrange
            StartBotSelection();

            // Act
            requestBack(_appUI, _inputService);

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_appUI.BotSelectionScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Returning_Home_from_bot_selection_detaches_from_back_requests()
        {
            // Arrange
            StartBotSelection();

            // Act
            _inputService.RaiseBack();

            // Assert
            Assert.That(_inputService.HasBackSubscribers, Is.False);
        }

        [Test]
        public void Disposing_at_bot_selection_detaches_from_application_back_requests()
        {
            // Arrange
            StartBotSelection();

            // Act
            _sut.Dispose();
            _inputService.RaiseBack();

            // Assert
            Assert.That(_inputService.HasBackSubscribers, Is.False);
            Assert.That(_appUI.LastRequestedRole, Is.EqualTo(typeof(IBotSelectionScreen)));
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
        }

        // --- PvE turn cycle ---

        [Test]
        public void A_terminal_PvE_X_placement_preserves_the_selected_setup_for_the_outcome()
        {
            // Arrange
            StartProfessionalPveGameplay();
            GameSetup selectedSetup = _appUI.GameplayScreen.ShownSetup;
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 1));

            // Act
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 2));
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_appUI.OutcomeScreen.ShownSetup, Is.SameAs(selectedSetup));
            Assert.That(_appUI.OutcomeScreen.ShownOutcome, Is.EqualTo(Outcome.XWin));
        }

        [Test]
        public void A_terminal_PvE_X_placement_starts_only_the_existing_outcome_delay()
        {
            // Arrange
            StartAmateurPveGameplay();
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 1));

            // Act
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 2));

            // Assert
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.XWin));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(1f));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
        }

        // --- Quit confirmation ---

        [Test]
        public void Opening_quit_confirmation_pauses_PvE_gameplay_before_showing_confirmation()
        {
            // Arrange
            StartAmateurPveGameplay();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _inputService.RaiseBack();

            // Assert
            AssertLastRequested<IConfirmQuitScreen>();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_boardView.ShownMarks, Is.EqualTo(new[]
            {
                (new CellCoordinate(0, 0), Mark.X),
            }));
        }

        [Test]
        public void Dismissing_quit_confirmation_resumes_PvE_gameplay()
        {
            // Arrange
            StartAmateurPveGameplay();
            _inputService.RaiseBack();

            // Act
            _appUI.ConfirmQuitScreen.ClickCancel();

            // Assert
            AssertLastClosed<IConfirmQuitScreen>();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_boardView.ShownMarks, Is.Empty);
        }

        [Test]
        public void Confirming_quit_during_a_pending_PvE_bot_turn_exits_gameplay_before_returning_Home()
        {
            // Arrange
            StartAmateurPveGameplay();
            _presser.Press(new CellCoordinate(0, 0));
            _inputService.RaiseBack();

            // Act
            _appUI.ConfirmQuitScreen.ClickConfirm();

            // Assert
            AssertLastRequested<IHomeScreen>();
            AssertLastClosed<IConfirmQuitScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 1)), Is.True);
        }

        [Test]
        public void Disposing_during_a_PvE_turn_exits_the_selected_controller()
        {
            // Arrange
            StartAmateurPveGameplay();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _sut.Dispose();

            // Assert
            Assert.That(_inputService.HasBackSubscribers, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(1));
        }

        [Test]
        public void Gameplay_back_opens_quit_confirmation_and_disables_board_presses()
        {
            // Arrange
            StartGameplay();

            // Act
            _appUI.GameplayScreen.ClickBack();

            // Assert
            AssertLastRequested<IConfirmQuitScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        private static IEnumerable<TestCaseData> QuitConfirmationDismissals()
        {
            yield return new TestCaseData((Action<FakeAppUI, FakeInputService>)((appUI, _) => appUI.ConfirmQuitScreen.ClickCancel()))
                .SetName("Dismissing_quit_confirmation_with_no_closes_it_and_restores_board_presses");

            yield return new TestCaseData((Action<FakeAppUI, FakeInputService>)((_, inputService) => inputService.RaiseBack()))
                .SetName("Dismissing_quit_confirmation_with_back_closes_it_and_restores_board_presses");
        }

        [TestCaseSource(nameof(QuitConfirmationDismissals))]
        public void Dismissing_quit_confirmation_closes_it_and_restores_board_presses(
            Action<FakeAppUI, FakeInputService> dismiss)
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            AssertLastRequested<IConfirmQuitScreen>();

            // Act
            dismiss(_appUI, _inputService);

            // Assert
            AssertLastClosed<IConfirmQuitScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Confirming_quit_returns_Home_with_the_game_preserved_and_board_presses_disabled()
        {
            // Arrange
            StartGameplay();
            _presser.Press(new CellCoordinate(0, 0));
            _inputService.RaiseBack();

            // Act
            _appUI.ConfirmQuitScreen.ClickConfirm();

            // Assert
            AssertLastRequested<IHomeScreen>();
            AssertLastClosed<IConfirmQuitScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.False);
        }

        // --- Board completion and the outcome delay ---

        [Test]
        public void Completing_the_board_disables_board_presses_and_requests_a_one_second_outcome_delay()
        {
            // Arrange
            StartGameplay();

            // Act
            BoardPresses.WinRowZeroForX(_presser);

            // Assert
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.XWin));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(1f));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
        }

        [Test]
        public void Back_during_the_pending_outcome_delay_does_not_open_quit_confirmation()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);

            // Act
            _inputService.RaiseBack();

            // Assert
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_delayScheduler.WasCancelled, Is.False);
        }

        // --- Outcome presentation and acknowledgement ---

        private static IEnumerable<TestCaseData> CompletedGames()
        {
            yield return new TestCaseData((Action<BoardPresser>)BoardPresses.WinRowZeroForX, Outcome.XWin)
                .SetName("The_delayed_outcome_presents_an_X_win_over_the_unchanged_completed_board");

            yield return new TestCaseData((Action<BoardPresser>)BoardPresses.WinRowZeroForO, Outcome.OWin)
                .SetName("The_delayed_outcome_presents_an_O_win_over_the_unchanged_completed_board");

            yield return new TestCaseData((Action<BoardPresser>)BoardPresses.FillForDraw, Outcome.Draw)
                .SetName("The_delayed_outcome_presents_a_draw_over_the_unchanged_completed_board");
        }

        [TestCaseSource(nameof(CompletedGames))]
        public void The_delayed_outcome_presents_the_captured_result_over_the_unchanged_completed_board(
            Action<BoardPresser> completeGame,
            Outcome expectedOutcome)
        {
            // Arrange
            StartGameplay();
            completeGame(_presser);
            Mark?[,] completedMarks = BoardState.CaptureMarks(_boardModel);
            List<(CellCoordinate Coordinate, Mark Mark)> renderedMarks = new(_boardView.ShownMarks);

            // Act
            _delayScheduler.FirePending();

            // Assert
            AssertLastRequested<IOutcomeScreen>();
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.OutcomeScreen.ShownOutcome, Is.EqualTo(expectedOutcome));
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(BoardState.CaptureMarks(_boardModel), Is.EqualTo(completedMarks));
            Assert.That(_boardView.ShownMarks, Is.EqualTo(renderedMarks));
        }

        private static IEnumerable<TestCaseData> OutcomeAcknowledgements()
        {
            yield return new TestCaseData((Action<FakeAppUI, FakeInputService>)((appUI, _) => appUI.OutcomeScreen.ClickContinue()))
                .SetName("Continuing_the_outcome_returns_Home_without_resetting_the_board");

            yield return new TestCaseData((Action<FakeAppUI, FakeInputService>)((_, inputService) => inputService.RaiseBack()))
                .SetName("Going_back_from_the_outcome_returns_Home_without_resetting_the_board");
        }

        [TestCaseSource(nameof(OutcomeAcknowledgements))]
        public void Acknowledging_the_outcome_returns_Home_without_resetting_the_board(
            Action<FakeAppUI, FakeInputService> acknowledge)
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            Mark?[,] completedMarks = BoardState.CaptureMarks(_boardModel);
            _delayScheduler.FirePending();

            // Act
            acknowledge(_appUI, _inputService);

            // Assert
            AssertLastRequested<IHomeScreen>();
            AssertLastClosed<IOutcomeScreen>();
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(BoardState.CaptureMarks(_boardModel), Is.EqualTo(completedMarks));
        }

        [Test]
        public void Replaying_the_delayed_callback_does_not_present_a_second_outcome()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            _delayScheduler.FirePending();

            // Act
            _delayScheduler.ReplayLastDelivered();

            // Assert
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(1));
            AssertLastRequested<IOutcomeScreen>();
        }

        [Test]
        public void A_late_delayed_callback_after_acknowledgement_does_not_present_another_outcome()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            _delayScheduler.FirePending();
            _appUI.OutcomeScreen.ClickContinue();

            // Act
            _delayScheduler.ReplayLastDelivered();

            // Assert
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(1));
            AssertLastClosed<IOutcomeScreen>();
            AssertLastRequested<IHomeScreen>();
        }

        // --- Teardown ---

        [Test]
        public void Disposing_while_the_outcome_delay_is_pending_cancels_it_and_detaches_back()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            Assert.That(_delayScheduler.HasPendingWork, Is.True);

            // Act
            _sut.Dispose();

            // Assert
            Assert.That(_delayScheduler.WasCancelled, Is.True);
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
            Assert.That(_inputService.HasBackSubscribers, Is.False);
        }

        [Test]
        public void Disposing_during_gameplay_detaches_the_board_completion_subscription()
        {
            // Arrange
            StartGameplay();

            // Act
            _sut.Dispose();
            BoardPresses.WinRowZeroForX(_presser);

            // Assert
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
        }
    }
}
