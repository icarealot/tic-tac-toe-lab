using System;
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

            _boardPresenter = BoardPresenterBuilder.Build(_boardModel, _boardLayout, _boardView, _inputService);
            _presser = new BoardPresser(_inputService, _boardLayout);
            _sut = new AppStateMachine(
                _boardPresenter,
                _appUI,
                _delayScheduler,
                _inputService,
                new FakeRandomService());
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

        private void StartProfessionalPveGameplay()
        {
            StartBotSelection();
            _appUI.BotSelectionScreen.ClickProfessional();
        }

        private void CompleteXWinThroughPresenter()
        {
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(1, 1));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 2));
        }

        private void AssertLastRequested<TScreen>() where TScreen : IScreen
        {
            Assert.That(_appUI.LastRequestedRole, Is.EqualTo(typeof(TScreen)));
        }

        private void AssertLastClosed<TScreen>() where TScreen : IScreen
        {
            Assert.That(_appUI.LastClosedRole, Is.EqualTo(typeof(TScreen)));
        }

        private static bool HasGameEndedSubscribers(BoardPresenter boardPresenter)
        {
            System.Reflection.FieldInfo gameEndedEventField = typeof(BoardPresenter).GetField(
                nameof(BoardPresenter.GameEnded),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(gameEndedEventField, Is.Not.Null, "BoardPresenter should expose its GameEnded event backing field to EditMode lifetime checks.");
            return gameEndedEventField.GetValue(boardPresenter) != null;
        }

        // --- Startup and Home ---

        [Test]
        public void Starting_the_state_machine_shows_Home_with_board_presses_disabled()
        {
            // Act
            _sut.Start();

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Starting_the_state_machine_offers_both_PvP_and_PvE_at_Home()
        {
            // Act
            _sut.Start();

            // Assert
            Assert.That(_appUI.HomeScreen.OnPvp, Is.Not.Null, "Home should offer the PvP choice.");
            Assert.That(_appUI.HomeScreen.OnPve, Is.Not.Null, "Home should offer the PvE choice.");
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
        public void Selecting_PvE_opens_bot_selection_instead_of_gameplay()
        {
            // Arrange
            _sut.Start();

            // Act
            _appUI.HomeScreen.ClickPve();

            // Assert
            AssertLastRequested<IBotSelectionScreen>();
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
            Assert.That(_appUI.GameplayScreen.ShownSetup.Mode, Is.EqualTo(GameMode.Pve));
            Assert.That(_appUI.GameplayScreen.ShownSetup.BotDifficulty, Is.EqualTo(BotDifficulty.Professional));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Clicking_back_at_bot_selection_returns_Home_without_confirmation()
        {
            // Arrange
            StartBotSelection();

            // Act
            _appUI.BotSelectionScreen.ClickBack();

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Raising_application_Back_at_bot_selection_returns_Home_without_confirmation()
        {
            // Arrange
            StartBotSelection();

            // Act
            _inputService.RaiseBack();

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Returning_Home_from_bot_selection_ignores_later_back_requests()
        {
            // Arrange
            StartBotSelection();
            int homePresentations = _appUI.HomeScreen.PresentationCount;

            // Act
            _inputService.RaiseBack();
            _inputService.RaiseBack();

            // Assert
            Assert.That(_inputService.HasBackSubscribers, Is.False);
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentations + 1));
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

        // --- Setup preservation and quit confirmation ---

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
        public void Opening_quit_confirmation_pauses_gameplay()
        {
            // Arrange
            StartGameplay();
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
        public void Dismissing_quit_confirmation_resumes_gameplay()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();

            // Act
            _appUI.ConfirmQuitScreen.ClickCancel();

            // Assert
            AssertLastClosed<IConfirmQuitScreen>();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Pressing_back_while_quit_confirmation_is_open_dismisses_it_and_resumes_gameplay()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();

            // Act
            _inputService.RaiseBack();

            // Assert
            AssertLastClosed<IConfirmQuitScreen>();
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
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.False);
        }

        [Test]
        public void Returning_Home_from_gameplay_detaches_from_later_back_requests()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            _appUI.ConfirmQuitScreen.ClickConfirm();
            int homePresentations = _appUI.HomeScreen.PresentationCount;

            // Act
            _inputService.RaiseBack();

            // Assert
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentations));
            Assert.That(_appUI.ConfirmQuitScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_inputService.HasBackSubscribers, Is.False);
        }

        [Test]
        public void Returning_Home_from_gameplay_detaches_from_later_game_end_events()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            _appUI.ConfirmQuitScreen.ClickConfirm();

            // Act
            bool hasGameEndedSubscribersAfterExit = HasGameEndedSubscribers(_boardPresenter);
            CompleteXWinThroughPresenter();

            // Assert
            Assert.That(hasGameEndedSubscribersAfterExit, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
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

        [Test]
        public void The_delayed_outcome_presents_the_X_win_over_the_unchanged_completed_board()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            Mark?[,] completedMarks = BoardState.CaptureMarks(_boardModel);

            // Act
            _delayScheduler.FirePending();

            // Assert
            AssertLastRequested<IOutcomeScreen>();
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(1));
            Assert.That(_appUI.OutcomeScreen.ShownOutcome, Is.EqualTo(Outcome.XWin));
            Assert.That(BoardState.CaptureMarks(_boardModel), Is.EqualTo(completedMarks));
        }

        // --- Outcome acknowledgement ---

        [Test]
        public void Continuing_the_outcome_returns_Home_without_resetting_the_board()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            Mark?[,] completedMarks = BoardState.CaptureMarks(_boardModel);
            _delayScheduler.FirePending();

            // Act
            _appUI.OutcomeScreen.ClickContinue();

            // Assert
            AssertLastRequested<IHomeScreen>();
            AssertLastClosed<IOutcomeScreen>();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(BoardState.CaptureMarks(_boardModel), Is.EqualTo(completedMarks));
        }

        [Test]
        public void Going_back_from_the_outcome_returns_Home_without_resetting_the_board()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            Mark?[,] completedMarks = BoardState.CaptureMarks(_boardModel);
            _delayScheduler.FirePending();

            // Act
            _inputService.RaiseBack();

            // Assert
            AssertLastRequested<IHomeScreen>();
            AssertLastClosed<IOutcomeScreen>();
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
        public void Returning_Home_from_outcome_ignores_later_back_requests()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            _delayScheduler.FirePending();
            _appUI.OutcomeScreen.ClickContinue();
            int homePresentations = _appUI.HomeScreen.PresentationCount;

            // Act
            _inputService.RaiseBack();

            // Assert
            AssertLastRequested<IHomeScreen>();
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentations));
            Assert.That(_inputService.HasBackSubscribers, Is.False);
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
        public void A_canceled_outcome_callback_after_disposal_does_not_present_the_outcome()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            Action staleCallback = _delayScheduler.CapturePendingCallback();
            _sut.Dispose();

            // Act
            staleCallback();

            // Assert
            Assert.That(_appUI.OutcomeScreen.PresentationCount, Is.EqualTo(0));
            Assert.That(_inputService.HasBackSubscribers, Is.False);
        }

        [Test]
        public void Disposing_during_gameplay_disables_presses_and_detaches_board_completion()
        {
            // Arrange
            StartGameplay();

            // Act
            _sut.Dispose();
            bool hasGameEndedSubscribersAfterDisposal = HasGameEndedSubscribers(_boardPresenter);
            CompleteXWinThroughPresenter();

            // Assert
            Assert.That(hasGameEndedSubscribersAfterDisposal, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_inputService.HasBackSubscribers, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
        }
    }
}
