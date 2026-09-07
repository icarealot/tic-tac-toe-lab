using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameplayStateTests
    {
        private GameplayPanel Panel => _fakeUIRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(includeInactive: true);
        private ConfirmQuitPopup Popup => _fakeUIRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(includeInactive: true);

        private FakeBoardSession _fakeBoardSession;
        private FakeInputService _fakeInputService;
        private FakeLogService _fakeLogService;
        private FakeCoroutineService _fakeCoroutineService;
        private FakeComponentFactoryService _fakeFactoryService;
        private FakeUIRoot _fakeUIRoot;
        private UIService _uiService;
        private FakeStateMachine _fakeStateMachine;
        private GameplayState _gameplayState;

        [SetUp]
        public void SetUp()
        {
            _fakeBoardSession = new FakeBoardSession();
            _fakeInputService = new FakeInputService();
            _fakeLogService = new FakeLogService();
            _fakeCoroutineService = new FakeCoroutineService();
            _fakeFactoryService = new FakeComponentFactoryService();
            _fakeUIRoot = new FakeUIRoot();
            _uiService = new UIService(_fakeFactoryService, _fakeUIRoot, _fakeCoroutineService);
            _fakeStateMachine = new FakeStateMachine();
            _gameplayState = new GameplayState(
                _fakeBoardSession, _fakeStateMachine, _uiService,
                _fakeInputService, _fakeLogService);
        }

        [Test]
        public void Entering_gameplay_shows_the_gameplay_panel()
        {
            _gameplayState.Enter();

            Assert.That(Panel, Is.Not.Null);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Leaving_gameplay_closes_the_gameplay_panel()
        {
            _gameplayState.Enter();

            _gameplayState.Leave();

            Assert.That(Panel, Is.Null);
        }

        [Test]
        public void The_gameplay_state_unsubscribes_from_the_session_ending_on_exit()
        {
            _gameplayState.Enter();
            _gameplayState.Leave();

            _fakeBoardSession.RaiseGameEnded();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void The_gameplay_state_enters_the_game_complete_state_when_the_ending_is_announced()
        {
            _gameplayState.Enter();

            _fakeBoardSession.RaiseGameEnded();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(GameCompleteState)));
            Assert.That(_fakeStateMachine.ChangeStateCount, Is.EqualTo(1));
        }

        [Test]
        public void Re_entering_gameplay_keeps_one_session_ending_subscription()
        {
            _gameplayState.Enter();
            _gameplayState.Leave();
            _gameplayState.Enter();

            _fakeBoardSession.RaiseGameEnded();

            Assert.That(_fakeStateMachine.ChangeStateCount, Is.EqualTo(1));
        }

        [Test]
        public void Back_during_a_game_opens_a_confirm_popup_and_leaves_the_gameplay_panel_on_its_stack()
        {
            _gameplayState.Enter();

            _gameplayState.Back();

            Assert.That(Popup, Is.Not.Null);
            Assert.That(Panel, Is.Not.Null);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void The_confirm_popup_floats_above_the_gameplay_panel_without_hiding_it()
        {
            _gameplayState.Enter();

            _gameplayState.Back();

            Assert.That(Popup.transform.parent, Is.EqualTo(_fakeUIRoot.PopupLayer));
            Assert.That(Popup.IsVisible, Is.True);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Back_again_closes_the_popup_and_opens_no_second_one()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            _gameplayState.Back();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(Popup, Is.Null);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Answering_no_closes_the_popup_and_leaves_gameplay_active()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            Popup.No();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(Panel.IsVisible, Is.True);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void Back_during_a_game_disables_gameplay_input()
        {
            _gameplayState.Enter();

            _gameplayState.Back();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.False);
        }

        [Test]
        public void Answering_no_re_enables_gameplay_input()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            Popup.No();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Back_again_re_enables_gameplay_input()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            _gameplayState.Back();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }

        [Test]
        public void Leaving_gameplay_with_the_popup_up_re_enables_gameplay_input()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            _gameplayState.Leave();

            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
        }
    }
}
