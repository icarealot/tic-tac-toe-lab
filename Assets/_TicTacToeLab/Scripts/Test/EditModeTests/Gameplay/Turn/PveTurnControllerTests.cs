using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class PveTurnControllerTests
    {
        private BoardModel _boardModel;
        private BoardLayout _boardLayout;
        private BoardPresenter _boardPresenter;
        private FakeBoardView _boardView;
        private FakeInputService _inputService;
        private FakeDelayScheduler _delayScheduler;
        private FakeRandomService _randomService;
        private DeterministicBot _bot;
        private BoardPresser _presser;
        private PveTurnController _sut;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _boardLayout = new BoardLayout(_boardModel.Dimension);
            _boardView = new FakeBoardView();
            _inputService = new FakeInputService();
            _delayScheduler = new FakeDelayScheduler();
            _randomService = new FakeRandomService();
            _bot = new DeterministicBot(new CellCoordinate(0, 1));
            _boardPresenter = BoardPresenterBuilder.Build(_boardModel, _boardLayout, _boardView, _inputService);
            _presser = new BoardPresser(_inputService, _boardLayout);
            _sut = new PveTurnController(
                _boardPresenter,
                _inputService,
                _delayScheduler,
                _randomService,
                _bot);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Exit();
            _boardPresenter.Dispose();
        }

        [Test]
        public void Entering_PvE_enables_X_interaction_without_waiting_for_a_turn_notification()
        {
            // Arrange
            _inputService.DisablePlayerPress();

            // Act
            _sut.Enter();

            // Assert
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_randomService.FloatingPointCallCount, Is.EqualTo(0));
        }

        [Test]
        public void A_nonterminal_X_placement_disables_presses_and_schedules_one_O_placement()
        {
            // Arrange
            _randomService.FloatingPointResult = 0.7f;
            _sut.Enter();

            // Act
            _presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(0, 0)), Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(0.7f));
            Assert.That(_randomService.FloatingPointCallCount, Is.EqualTo(1));
            Assert.That(_randomService.LastFloatingPointMinimumInclusive, Is.EqualTo(0.4f));
            Assert.That(_randomService.LastFloatingPointMaximumInclusive, Is.EqualTo(1f));
        }

        [TestCase(0.4f)]
        [TestCase(0.7f)]
        [TestCase(1f)]
        public void Each_new_bot_turn_uses_the_inclusive_delay_range(float delaySeconds)
        {
            // Arrange
            _randomService.FloatingPointResult = delaySeconds;
            _sut.Enter();

            // Act
            _presser.Press(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(delaySeconds));
            Assert.That(_randomService.LastFloatingPointMinimumInclusive, Is.EqualTo(0.4f));
            Assert.That(_randomService.LastFloatingPointMaximumInclusive, Is.EqualTo(1f));
        }

        [Test]
        public void Repeated_O_turn_notifications_keep_only_one_bot_placement_pending()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 1));

            // Act
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 2));

            // Assert
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_randomService.FloatingPointCallCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
        }

        [Test]
        public void Current_bot_work_places_one_O_and_restores_X_interaction()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(0, 1)), Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_bot.SelectionCount, Is.EqualTo(1));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        [Test]
        public void Replaying_delivered_bot_work_does_not_place_a_second_O()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            _delayScheduler.FirePending();

            // Act
            _delayScheduler.ReplayLastDelivered();

            // Assert
            Assert.That(_bot.SelectionCount, Is.EqualTo(1));
            Assert.That(_boardView.ShownMarks, Has.Count.EqualTo(2));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Pausing_PvE_cancels_pending_O_work_and_disables_presses()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _sut.Pause();

            // Assert
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
            Assert.That(_delayScheduler.WasCancelled, Is.True);
        }

        [Test]
        public void A_captured_callback_after_PvE_pause_cannot_place_an_O()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            Action staleCallback = _delayScheduler.CapturePendingCallback();
            _sut.Pause();

            // Act
            staleCallback();

            // Assert
            Assert.That(_bot.SelectionCount, Is.EqualTo(0));
            Assert.That(_boardView.ShownMarks, Has.Count.EqualTo(1));
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        [Test]
        public void Resuming_an_interrupted_O_turn_starts_a_fresh_delay()
        {
            // Arrange
            _randomService.FloatingPointResult = 0.4f;
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            _sut.Pause();
            _randomService.FloatingPointResult = 1f;

            // Act
            _sut.Resume();

            // Assert
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(2));
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(1f));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
        }

        [Test]
        public void A_stale_callback_after_PvE_pause_cannot_disturb_newer_bot_work()
        {
            // Arrange
            _randomService.FloatingPointResult = 0.4f;
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            Action staleCallback = _delayScheduler.CapturePendingCallback();
            _sut.Pause();
            _randomService.FloatingPointResult = 1f;
            _sut.Resume();

            // Act
            staleCallback();

            // Assert
            Assert.That(_bot.SelectionCount, Is.EqualTo(0));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(2));
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Resuming_an_X_turn_restores_presses_without_scheduling_bot_work()
        {
            // Arrange
            _sut.Enter();
            _sut.Pause();

            // Act
            _sut.Resume();

            // Assert
            Assert.That(_inputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_randomService.FloatingPointCallCount, Is.EqualTo(0));
        }

        [Test]
        public void Turn_changes_while_paused_are_ignored_without_scheduling_bot_work()
        {
            // Arrange
            _sut.Enter();
            _sut.Pause();

            // Act
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Resuming_after_a_paused_turn_change_schedules_the_O_turn()
        {
            // Arrange
            _sut.Enter();
            _sut.Pause();
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 0));
            _randomService.FloatingPointResult = 0.7f;

            // Act
            _sut.Resume();

            // Assert
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.RequestedDelaySeconds, Is.EqualTo(0.7f));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.HasPendingWork, Is.True);
        }

        [Test]
        public void Exiting_PvE_cancels_bot_work_and_disables_presses()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));

            // Act
            _sut.Exit();

            // Assert
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
            Assert.That(_delayScheduler.WasCancelled, Is.True);
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        [Test]
        public void A_captured_callback_after_PvE_exit_cannot_place_an_O()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            Action staleCallback = _delayScheduler.CapturePendingCallback();
            _sut.Exit();

            // Act
            staleCallback();

            // Assert
            Assert.That(_bot.SelectionCount, Is.EqualTo(0));
            Assert.That(_boardView.ShownMarks, Has.Count.EqualTo(1));
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        [Test]
        public void Turn_changes_after_PvE_exit_do_not_schedule_bot_work()
        {
            // Arrange
            _sut.Enter();
            _sut.Exit();

            // Act
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(0));
            Assert.That(_randomService.FloatingPointCallCount, Is.EqualTo(0));
            Assert.That(_inputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Current_bot_work_is_ignored_when_the_turn_is_no_longer_O()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            Action callback = _delayScheduler.CapturePendingCallback();
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 1));

            // Act
            callback();

            // Assert
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_bot.SelectionCount, Is.EqualTo(0));
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        [Test]
        public void Current_bot_work_is_ignored_when_the_outcome_is_terminal()
        {
            // Arrange
            _sut.Enter();
            _presser.Press(new CellCoordinate(0, 0));
            Action callback = _delayScheduler.CapturePendingCallback();
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(1, 1));
            _ = _boardPresenter.TryPlaceMark(new CellCoordinate(0, 2));

            // Act
            callback();

            // Assert
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.XWin));
            Assert.That(_bot.SelectionCount, Is.EqualTo(0));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        [Test]
        public void A_terminal_O_placement_clears_bot_work_before_the_synchronous_game_end()
        {
            // Arrange
            _bot.Placement = new CellCoordinate(0, 2);
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 0));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(1, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(0, 1));
            _ = _boardModel.TryPlaceMark(new CellCoordinate(2, 2));
            _sut.Enter();

            // Act
            _delayScheduler.FirePending();

            // Assert
            Assert.That(_boardModel.GetMark(new CellCoordinate(0, 2)), Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.OWin));
            Assert.That(_bot.SelectionCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(_delayScheduler.HasPendingWork, Is.False);
        }

        private sealed class DeterministicBot : IBot
        {
            public CellCoordinate Placement { get; set; }
            public int SelectionCount { get; private set; }

            public DeterministicBot(CellCoordinate coordinate)
            {
                Placement = coordinate;
            }

            public CellCoordinate SelectPlacement(BoardModel boardModel)
            {
                SelectionCount++;
                return Placement;
            }
        }
    }
}
