using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    // These tests deliberately wire the real board and state graph. The graph, rather than any one
    // fake, is what makes the subscription coverage valuable.
    public sealed class GameplayStateWiringTests
    {
        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private FakeCoroutineService _fakeCoroutineService;
        private FakeFactoryService _fakeFactory;
        private BoardSession _boardSession;
        private UIService _uiService;
        private AppStateMachine _stateMachine;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            BoardPresenter boardPresenter = new(_boardModel,
                                                _fakeBoardView,
                                                _fakeInputService,
                                                new FakeCameraService(),
                                                new FakeLogService());
            _boardSession = new BoardSession(boardPresenter);
            _fakeCoroutineService = new FakeCoroutineService();
            _fakeFactory = new FakeFactoryService();
            _uiService = new UIService(_fakeFactory, new FakeUIRoot(), _fakeCoroutineService);
            _stateMachine = new AppStateMachine(_fakeInputService);

            MainMenuState mainMenuState = new(_boardSession,
                                            _stateMachine,
                                            _uiService);
            GameplayState gameplayState = new(_boardSession,
                                            _stateMachine,
                                            _uiService,
                                            _fakeInputService);
            GameCompleteState gameCompleteState = new(_boardSession,
                                                    _stateMachine,
                                                    _fakeCoroutineService);
            _stateMachine.Add(mainMenuState);
            _stateMachine.Add(gameplayState);
            _stateMachine.Add(gameCompleteState);
        }

        [TearDown]
        public void TearDown()
        {
            _stateMachine?.Dispose();
            _boardSession?.Dispose();
        }

        [Test]
        public void Re_entering_gameplay_after_each_game_does_not_accumulate_subscriptions()
        {
            _stateMachine.ChangeState<GameplayState>();

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();

            // An extra, leftover subscription from a prior game would clear the board more than once per ending.
            Assert.That(_fakeBoardView.ClearCount, Is.EqualTo(3));
        }

        [Test]
        public void Many_games_in_a_row_leave_exactly_one_active_subscription()
        {
            _stateMachine.ChangeState<GameplayState>();

            for (int i = 0; i < 5; i++)
            {
                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.FireScheduledCallback();
            }

            _boardSession.Dispose();

            Assert.That(_fakeInputService.HasSubscribers, Is.False);
        }

        [Test]
        public void A_game_that_ends_while_the_popup_is_up_leaves_the_phase_with_its_popups_cleared()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            Assert.That(() =>
            {
                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.FireScheduledCallback();
            }, Throws.Nothing);

            Assert.That(_uiService.HasPopup, Is.False);
        }

        [Test]
        public void Tearing_down_the_machine_leaves_gameplay_so_a_subsequent_ending_reaches_nothing()
        {
            _stateMachine.ChangeState<GameplayState>();

            _stateMachine.Dispose();
            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);

            Assert.That(_fakeBoardView.WasCleared, Is.False);
            Assert.That(_boardModel.Outcome, Is.Not.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void A_panel_back_press_then_yes_round_trip_lands_on_the_menu_with_all_windows_cleaned_up()
        {
            _stateMachine.ChangeState<MainMenuState>();
            _fakeFactory.MenuPanels[0].StartGame();

            _fakeFactory.Panels[0].Back();
            _fakeFactory.Popups[0].Yes();

            Assert.That(_fakeFactory.MenuPanels, Has.Count.EqualTo(2));
            Assert.That(_fakeFactory.Panels, Has.Count.EqualTo(1));
            Assert.That(_fakeFactory.Popups, Has.Count.EqualTo(1));
            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(_fakeFactory.Panels[0]));
            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(_fakeFactory.Popups[0]));
        }

        [Test]
        public void The_full_round_trip_through_the_menu_ends_on_a_fresh_board()
        {
            _stateMachine.ChangeState<MainMenuState>();
            _fakeFactory.MenuPanels[0].StartGame();

            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);
            Assert.That(_boardModel.IsEmpty(0, 0), Is.False);

            _fakeInputService.RaiseBack();
            _fakeFactory.Popups[0].Yes();

            // Back on the menu, the abandoned board keeps its mark hidden behind the menu panel.
            Assert.That(_fakeFactory.MenuPanels, Has.Count.EqualTo(2));
            Assert.That(_fakeFactory.Popups, Has.Count.EqualTo(1));
            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
            Assert.That(_boardModel.IsEmpty(0, 0), Is.False);

            _fakeFactory.MenuPanels[1].StartGame();

            // The next Start begins from an empty board with X to play, and the reset reached the view.
            Assert.That(_boardModel.IsEmpty(0, 0), Is.True);
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.ClearCount, Is.EqualTo(2));
        }

        [Test]
        public void The_end_of_game_loop_returns_to_gameplay_without_the_menu()
        {
            _stateMachine.ChangeState<MainMenuState>();
            _fakeFactory.MenuPanels[0].StartGame();

            BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
            _fakeCoroutineService.FireScheduledCallback();

            // The pause returned to gameplay — a second gameplay panel was shown and no menu panel
            // was ever requested again.
            Assert.That(_fakeFactory.Panels, Has.Count.EqualTo(2));
            Assert.That(_fakeFactory.MenuPanels, Has.Count.EqualTo(1));
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Repeated_round_trips_through_the_menu_clear_the_board_exactly_once_per_game_ending()
        {
            for (int i = 0; i < 3; i++)
            {
                _stateMachine.ChangeState<MainMenuState>();
                _fakeFactory.MenuPanels[_fakeFactory.MenuPanels.Count - 1].StartGame();

                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.FireScheduledCallback();
            }

            // Each round contributes exactly two clears: the Start reset and the ending reset.
            // A subscription left behind by an earlier round trip would clear the board again.
            Assert.That(_fakeBoardView.ClearCount, Is.EqualTo(6));
        }
    }
}
