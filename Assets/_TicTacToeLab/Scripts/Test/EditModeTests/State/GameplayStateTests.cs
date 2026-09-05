using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameplayStateTests
    {
        private GameplayPanel Panel => _fakeUIRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(includeInactive: true);
        private ConfirmQuitPopup Popup => _fakeUIRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(includeInactive: true);

        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private FakeLogService _fakeLogService;
        private FakeCoroutineService _fakeCoroutineService;
        private FakeFactoryService _fakeFactoryService;
        private BoardSession _boardSession;
        private FakeUIRoot _fakeUIRoot;
        private UIService _uiService;
        private AppStateMachine _stateMachine;
        private GameplayState _gameplayState;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            _fakeLogService = new FakeLogService();
            BoardPresenter boardPresenter = new(
                _boardModel, _fakeBoardView,
                _fakeInputService, new FakeCameraService(), _fakeLogService);
            _boardSession = new BoardSession(boardPresenter);
            _fakeCoroutineService = new FakeCoroutineService();
            _fakeFactoryService = new FakeFactoryService();
            _fakeUIRoot = new FakeUIRoot();
            _uiService = new UIService(_fakeFactoryService, _fakeUIRoot, _fakeCoroutineService);
            _stateMachine = new AppStateMachine(_fakeInputService);
            _gameplayState = new GameplayState(_boardSession, _stateMachine, _uiService, _fakeInputService, _fakeLogService);
            _stateMachine.Add(_gameplayState);
            _stateMachine.Add(new GameCompleteState(_boardSession, _stateMachine, _fakeCoroutineService));
        }

        [Test]
        public void Entering_gameplay_shows_the_gameplay_panel()
        {
            _stateMachine.ChangeState<GameplayState>();

            Assert.That(Panel, Is.Not.Null);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Leaving_gameplay_closes_the_gameplay_panel()
        {
            _stateMachine.ChangeState<GameplayState>();

            _gameplayState.Leave();

            Assert.That(Panel, Is.Null);
        }

        [Test]
        public void Back_during_a_game_opens_a_confirm_popup_and_leaves_the_gameplay_panel_on_its_stack()
        {
            _stateMachine.ChangeState<GameplayState>();

            _fakeInputService.RaiseBack();

            Assert.That(Popup, Is.Not.Null);
            Assert.That(Panel, Is.Not.Null);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void The_confirm_popup_floats_above_the_gameplay_panel_without_hiding_it()
        {
            _stateMachine.ChangeState<GameplayState>();

            _fakeInputService.RaiseBack();

            Assert.That(Popup.transform.parent, Is.EqualTo(_fakeUIRoot.PopupLayer));
            Assert.That(Popup.IsVisible, Is.True);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Back_again_closes_the_popup_and_opens_no_second_one()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            _fakeInputService.RaiseBack();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(Popup, Is.Null);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Answering_no_closes_the_popup_and_the_game_continues_exactly_as_it_was()
        {
            _stateMachine.ChangeState<GameplayState>();
            BoardMoves.PressCell(_fakeInputService, _boardModel, 0, 0);
            _fakeInputService.RaiseBack();

            Popup.No();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(Panel.IsVisible, Is.True);
            Assert.That(_boardModel.GetMark(0, 0), Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));

            // The game is still the one that was interrupted: the next press plays on into it.
            BoardMoves.PressCell(_fakeInputService, _boardModel, 1, 1);
            Assert.That(_boardModel.GetMark(1, 1), Is.EqualTo(Mark.O));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void Back_during_a_game_disables_gameplay_input()
        {
            _stateMachine.ChangeState<GameplayState>();

            _fakeInputService.RaiseBack();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Answering_no_re_enables_gameplay_input()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            Popup.No();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Back_again_re_enables_gameplay_input()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            _fakeInputService.RaiseBack();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void A_game_that_ends_while_the_popup_is_up_leaves_the_phase_with_its_popups_cleared()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            // The one sequence ADR 0008 warns about: the game ends underneath an open popup, and the
            // next phase wants to show a panel. Clearing on the way out is what keeps that from throwing.
            Assert.That(() =>
            {
                BoardMoves.WinRowZeroForX(_fakeInputService, _boardModel);
                _fakeCoroutineService.PumpToCompletion();
            }, Throws.Nothing);

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(Panel, Is.Not.Null);
        }

        [Test]
        public void Leaving_gameplay_with_the_popup_up_re_enables_gameplay_input()
        {
            _stateMachine.ChangeState<GameplayState>();
            _fakeInputService.RaiseBack();

            _gameplayState.Leave();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }
    }
}
