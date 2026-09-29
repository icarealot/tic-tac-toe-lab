using System;
using System.Collections.Generic;
using System.Linq;
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
        private FakeRandomChoiceSource _randomChoiceSource;
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
            _randomChoiceSource = new FakeRandomChoiceSource();

            _boardPresenter = BoardPresenterBuilder.Build(_boardModel, _boardLayout, _boardView, _inputService);
            _presser = new BoardPresser(_inputService, _boardLayout);
            _sut = new AppStateMachine(_boardPresenter, _appUI, _delayScheduler, _inputService, _randomChoiceSource);
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

        private void StartPveGameplay(BotDifficulty botDifficulty = BotDifficulty.Amateur)
        {
            StartBotSelection();

            if (botDifficulty == BotDifficulty.Amateur)
            {
                _appUI.BotSelectionScreen.ClickAmateur();
            }
            else
            {
                _appUI.BotSelectionScreen.ClickProfessional();
            }
        }

        // --- Startup and Home ---

        [Test]
        public void Starting_the_state_machine_shows_Home_with_board_presses_disabled()
        {
            // Act
            _sut.Start();

            // Assert
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
            Assert.That(_appUI.HasPopup, Is.False);
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
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentationsBefore));
        }

        // --- Starting a game ---

        [Test]
        public void Starting_a_game_resets_the_existing_board_before_showing_gameplay_and_enables_board_presses()
        {
            // Arrange
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 2));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.XWin));

            // Act
            _sut.Start();
            _appUI.HomeScreen.ClickPvp();

            // Assert
            Assert.That(BoardState.IsEmpty(_boardModel), Is.True);
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_appUI.GameplayScreen.BoardOutcomeWhenPresented, Is.EqualTo(Outcome.InProgress));
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
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
            StartPveGameplay(BotDifficulty.Professional);

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
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
            Assert.That(_appUI.BotSelectionScreen.IsVisible, Is.False);
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
            Assert.That(_randomChoiceSource.CallCount, Is.EqualTo(0));
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
            Assert.That(_appUI.BotSelectionScreen.IsVisible, Is.True);
            Assert.That(_appUI.HomeScreen.IsVisible, Is.False);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [TestCase(BotDifficulty.Amateur)]
        [TestCase(BotDifficulty.Professional)]
        public void Selecting_a_bot_difficulty_starts_gameplay_with_that_PvE_setup_unchanged(BotDifficulty botDifficulty)
        {
            // Arrange
            StartBotSelection();

            // Act
            if (botDifficulty == BotDifficulty.Amateur)
            {
                _appUI.BotSelectionScreen.ClickAmateur();
            }
            else
            {
                _appUI.BotSelectionScreen.ClickProfessional();
            }

            // Assert
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
            Assert.That(_appUI.BotSelectionScreen.IsVisible, Is.False);
            Assert.That(_appUI.GameplayScreen.ShownSetup.Mode, Is.EqualTo(GameMode.Pve));
            Assert.That(_appUI.GameplayScreen.ShownSetup.BotDifficulty, Is.EqualTo(botDifficulty));
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
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.BotSelectionScreen.IsVisible, Is.False);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Repeating_a_bot_difficulty_choice_starts_gameplay_once()
        {
            // Arrange
            StartBotSelection();
            int gameplayPresentationsBefore = _appUI.GameplayScreen.PresentationCount;

            // Act
            _appUI.BotSelectionScreen.ClickAmateur();
            _appUI.BotSelectionScreen.ClickProfessional();

            // Assert
            Assert.That(_appUI.GameplayScreen.PresentationCount, Is.EqualTo(gameplayPresentationsBefore + 1));
            Assert.That(_appUI.GameplayScreen.ShownSetup.BotDifficulty, Is.EqualTo(BotDifficulty.Amateur));
            Assert.That(_appUI.HomeScreen.IsVisible, Is.False);
        }

        [Test]
        public void Repeating_back_at_bot_selection_returns_Home_once()
        {
            // Arrange
            StartBotSelection();
            int homePresentationsBefore = _appUI.HomeScreen.PresentationCount;

            // Act
            _appUI.BotSelectionScreen.ClickBack();
            _inputService.RaiseBack();
            _appUI.BotSelectionScreen.ClickBack();

            // Assert
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentationsBefore + 1));
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
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
        public void A_stale_bot_difficulty_choice_after_returning_Home_does_not_start_gameplay()
        {
            // Arrange
            StartBotSelection();
            Action staleAmateur = _appUI.BotSelectionScreen.OnAmateur;
            _appUI.BotSelectionScreen.ClickBack();

            // Act
            staleAmateur();

            // Assert
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
        }

        [Test]
        public void A_stale_bot_selection_back_callback_after_starting_a_game_does_not_return_Home()
        {
            // Arrange
            StartBotSelection();
            Action staleBack = _appUI.BotSelectionScreen.OnBack;
            _appUI.BotSelectionScreen.ClickAmateur();

            // Act
            staleBack();

            // Assert
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
            Assert.That(_appUI.HomeScreen.IsVisible, Is.False);
            Assert.That(_appUI.HasPopup, Is.False);
        }

        [Test]
        public void Disposing_at_bot_selection_detaches_back_and_keeps_every_callback_inert()
        {
            // Arrange
            StartBotSelection();
            Action staleAmateur = _appUI.BotSelectionScreen.OnAmateur;
            Action staleBack = _appUI.BotSelectionScreen.OnBack;

            // Act
            _sut.Dispose();
            staleAmateur();
            staleBack();
            _inputService.RaiseBack();

            // Assert
            Assert.That(_inputService.HasBackSubscribers, Is.False);
            Assert.That(_appUI.HomeScreen.IsVisible, Is.False);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
        }

        // --- PvE turn cycle ---

        [Test]
        public void A_successful_nonterminal_PvE_X_placement_schedules_one_bot_turn_within_the_agreed_delay()
        {
            // Arrange
            StartPveGameplay();

            // Act
            _presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(0, 0)), Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.InRange(0.4f, 1f));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Firing_the_pending_PvE_bot_turn_places_and_renders_one_O_then_restores_X_interaction()
        {
            // Arrange
            StartPveGameplay();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(0, 1)), Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_boardView.ShownMarks, Is.EqualTo(new[]
            {
                (new CellCoordinate(0, 0), Mark.X),
                (new CellCoordinate(0, 1), Mark.O),
            }));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
        }

        [TestCase(0, 0.4f)]
        [TestCase(6, 1f)]
        public void A_PvE_bot_delay_includes_the_agreed_bounds(int randomIndex, float expectedDelaySeconds)
        {
            // Arrange
            StartPveGameplay();
            _randomChoiceSource.Index = randomIndex;

            // Act
            _presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(expectedDelaySeconds));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void Every_other_controlled_PvE_bot_delay_stays_within_the_agreed_range(int randomIndex)
        {
            // Arrange
            StartPveGameplay();
            _randomChoiceSource.Index = randomIndex;

            // Act
            _presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.InRange(0.4f, 1f));
        }

        [Test]
        public void A_rejected_PvE_board_press_schedules_no_bot_turn()
        {
            // Arrange
            StartPveGameplay();

            // Act
            _inputService.RaisePress(new UnityEngine.Vector2(3f, 3f));

            // Assert
            Assert.That(BoardState.IsEmpty(_boardModel), Is.True);
            Assert.That(_boardView.ShownMarks, Is.Empty);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Board_presses_are_ignored_while_a_PvE_bot_turn_is_pending()
        {
            // Arrange
            StartPveGameplay();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _presser.Press(new CellCoordinate(0, 1));

            // Assert
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 1)), Is.True);
            Assert.That(_boardView.ShownMarks, Is.EqualTo(new[]
            {
                (new CellCoordinate(0, 0), Mark.X),
            }));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
        }

        [Test]
        public void A_terminal_PvE_X_placement_starts_only_the_existing_outcome_delay()
        {
            // Arrange
            StartPveGameplay();
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
            Assert.That(_appUI.OutcomeScreen.IsVisible, Is.False);
        }

        [Test]
        public void Professional_PvE_uses_its_strategy_for_the_delayed_O_placement()
        {
            // Arrange
            StartPveGameplay(BotDifficulty.Professional);
            _presser.Press(new CellCoordinate(2, 2));

            // Act
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(1, 1)), Is.EqualTo(Mark.O));
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.True);
            Assert.That(_boardView.ShownMarks[^1], Is.EqualTo((new CellCoordinate(1, 1), Mark.O)));
        }

        [Test]
        public void A_terminal_PvE_O_placement_ends_the_bot_delay_before_starting_a_separate_outcome_delay()
        {
            // Arrange
            StartPveGameplay();
            _presser.Press(new CellCoordinate(1, 0));
            _delayScheduler.FirePending();
            _presser.Press(new CellCoordinate(2, 0));
            _delayScheduler.FirePending();
            _presser.Press(new CellCoordinate(2, 1));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(3));

            // Act
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(0, 2)), Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.OWin));
            Assert.That(_boardView.ShownMarks[^1], Is.EqualTo((new CellCoordinate(0, 2), Mark.O)));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(4));
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(1f));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_appUI.OutcomeScreen.IsVisible, Is.False);
        }

        // --- Quit confirmation ---

        [Test]
        public void Gameplay_back_opens_quit_confirmation_and_blocks_board_presses()
        {
            // Arrange
            StartGameplay();

            // Act
            _appUI.GameplayScreen.ClickBack();

            // Assert
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);

            // Act
            _presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.True);
            Assert.That(_boardView.ShownMarks, Is.Empty);
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
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.True);

            // Act
            dismiss(_appUI, _inputService);

            // Assert
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.False);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
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
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.False);
        }

        [Test]
        public void Confirming_quit_closes_confirm_quit_before_showing_Home()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            int operationsBefore = _appUI.Operations.Count;

            // Act
            _appUI.ConfirmQuitScreen.ClickConfirm();

            // Assert
            Assert.That(
                _appUI.Operations.Skip(operationsBefore).ToArray(),
                Is.EqualTo(new[] { "Close IConfirmQuitScreen", "Show IHomeScreen" }),
                "Confirming quit should close the confirmation before Home can present.");
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
            Assert.That(_appUI.OutcomeScreen.IsVisible, Is.False);
        }

        [Test]
        public void Back_during_the_pending_outcome_delay_has_no_effect()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);

            // Act
            _inputService.RaiseBack();

            // Assert
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_delayScheduler.WasCancelled, Is.False);

            // Act
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_appUI.OutcomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.False);
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
            Assert.That(_appUI.OutcomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.OutcomeScreen.ShownOutcome, Is.EqualTo(expectedOutcome));
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True, "The popup should appear over the still-visible gameplay screen.");
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
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.False);
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(BoardState.CaptureMarks(_boardModel), Is.EqualTo(completedMarks));
        }

        [Test]
        public void Acknowledging_the_outcome_closes_it_before_showing_Home()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            _delayScheduler.FirePending();
            int operationsBefore = _appUI.Operations.Count;

            // Act
            _appUI.OutcomeScreen.ClickContinue();

            // Assert
            Assert.That(
                _appUI.Operations.Skip(operationsBefore).ToArray(),
                Is.EqualTo(new[] { "Close IOutcomeScreen", "Show IHomeScreen" }),
                "Acknowledging the outcome should close it before Home can present.");
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
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
        }

        [Test]
        public void Acknowledging_the_outcome_repeatedly_causes_only_one_return_Home()
        {
            // Arrange
            StartGameplay();
            BoardPresses.WinRowZeroForX(_presser);
            _delayScheduler.FirePending();
            int homePresentationsBefore = _appUI.HomeScreen.PresentationCount;

            // Act
            _appUI.OutcomeScreen.ClickContinue();
            _appUI.OutcomeScreen.ClickContinue();
            _inputService.RaiseBack();

            // Assert
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentationsBefore + 1));
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
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
            Assert.That(_appUI.OutcomeScreen.IsVisible, Is.False);
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
        }

        [Test]
        public void A_stale_gameplay_back_callback_after_quitting_does_not_open_another_popup()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            _appUI.ConfirmQuitScreen.ClickConfirm();
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);

            // Act
            _appUI.GameplayScreen.ClickBack();

            // Assert
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_appUI.HomeScreen.IsVisible, Is.True);
        }

        [Test]
        public void A_stale_home_choice_during_gameplay_does_not_restart_the_game()
        {
            // Arrange
            StartGameplay();
            _presser.Press(new CellCoordinate(0, 0));
            int homePresentationsBefore = _appUI.HomeScreen.PresentationCount;

            // Act
            _appUI.HomeScreen.ClickPvp();
            _appUI.HomeScreen.ClickPve();

            // Assert
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentationsBefore));
            Assert.That(_boardModel.IsEmpty(new CellCoordinate(0, 0)), Is.False, "The in-progress board should not be reset.");
        }

        [Test]
        public void A_stale_quit_cancel_callback_after_returning_Home_does_not_enable_board_presses()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            _appUI.ConfirmQuitScreen.ClickConfirm();
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);

            // Act
            _appUI.ConfirmQuitScreen.ClickCancel();

            // Assert
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        // --- Stale confirmation callbacks ---

        private static IEnumerable<TestCaseData> DisposedConfirmationCallbacks()
        {
            yield return new TestCaseData((Action<FakeAppUI>)(appUI => appUI.ConfirmQuitScreen.ClickConfirm()))
                .SetName("Replaying_yes_from_a_confirmation_open_at_disposal_keeps_the_state_machine_inert");

            yield return new TestCaseData((Action<FakeAppUI>)(appUI => appUI.ConfirmQuitScreen.ClickCancel()))
                .SetName("Replaying_no_from_a_confirmation_open_at_disposal_keeps_the_state_machine_inert");
        }

        [TestCaseSource(nameof(DisposedConfirmationCallbacks))]
        public void Replaying_a_confirmation_callback_after_disposal_keeps_the_state_machine_inert(Action<FakeAppUI> replay)
        {
            // Arrange
            StartGameplay();
            _presser.Press(new CellCoordinate(0, 0));
            _inputService.RaiseBack();
            Mark?[,] marksBefore = BoardState.CaptureMarks(_boardModel);
            int homePresentationsBefore = _appUI.HomeScreen.PresentationCount;
            _sut.Dispose();

            // Act
            replay(_appUI);

            // Assert
            Assert.That(_appUI.HomeScreen.IsVisible, Is.False);
            Assert.That(_appUI.HomeScreen.PresentationCount, Is.EqualTo(homePresentationsBefore));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(BoardState.CaptureMarks(_boardModel), Is.EqualTo(marksBefore));
        }

        [Test]
        public void Replaying_the_closed_confirmations_confirm_callback_cannot_act_on_the_reopened_confirmation()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            Action staleConfirm = _appUI.ConfirmQuitScreen.OnConfirm;
            _appUI.ConfirmQuitScreen.ClickCancel();
            _inputService.RaiseBack();
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.True);

            // Act
            staleConfirm();

            // Assert
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.True);
            Assert.That(_appUI.HomeScreen.IsVisible, Is.False);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Replaying_the_closed_confirmations_cancel_callback_cannot_close_the_reopened_confirmation()
        {
            // Arrange
            StartGameplay();
            _inputService.RaiseBack();
            Action staleCancel = _appUI.ConfirmQuitScreen.OnCancel;
            _appUI.ConfirmQuitScreen.ClickCancel();
            _inputService.RaiseBack();
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.True);

            // Act
            staleCancel();

            // Assert
            Assert.That(_appUI.ConfirmQuitScreen.IsVisible, Is.True);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
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

            // Act
            _delayScheduler.FirePending();
            _inputService.RaiseBack();

            // Assert
            Assert.That(_appUI.HasPopup, Is.False);
            Assert.That(_appUI.GameplayScreen.IsVisible, Is.True);
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
