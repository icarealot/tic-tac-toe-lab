using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameplayStateTests
    {
        private FakeGameplayPanel Panel => _fakeFactory.Panels[0];
        private FakeConfirmQuitPopup Popup => _fakeFactory.Popups[0];

        private FakeBoardSession _fakeBoardSession;
        private FakeInputService _fakeInputService;
        private FakeFactoryService _fakeFactory;
        private UIService _uiService;
        private FakeStateMachine _fakeStateMachine;
        private GameplayState _gameplayState;

        [SetUp]
        public void SetUp()
        {
            _fakeBoardSession = new FakeBoardSession();
            _fakeInputService = new FakeInputService();
            _fakeFactory = new FakeFactoryService();
            _uiService = new UIService(_fakeFactory,
                                        new FakeUIRoot(),
                                        new FakeCoroutineService());
            _fakeStateMachine = new FakeStateMachine();
            _gameplayState = new GameplayState(_fakeBoardSession,
                                                _fakeStateMachine,
                                                _uiService,
                                                _fakeInputService);
        }

        [Test]
        public void Entering_gameplay_asks_for_the_gameplay_panel_by_interface_and_configures_it()
        {
            _gameplayState.Enter();

            Assert.That(_fakeFactory.Panels, Has.Count.EqualTo(1));
            Assert.That(Panel.ConfiguredBoardSession, Is.SameAs(_fakeBoardSession));
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Leaving_gameplay_closes_the_gameplay_panel()
        {
            _gameplayState.Enter();

            _gameplayState.Leave();

            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(Panel));
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

            Assert.That(_fakeFactory.Popups, Has.Count.EqualTo(1));
            Assert.That(_fakeFactory.ReturnedWindows.Contains(Panel), Is.False);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void The_confirm_popup_floats_above_the_gameplay_panel_without_hiding_it()
        {
            _gameplayState.Enter();

            _gameplayState.Back();

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
            Assert.That(_fakeFactory.Popups, Has.Count.EqualTo(1));
            Assert.That(_fakeFactory.ReturnedWindows.Contains(Popup), Is.True);
            Assert.That(Panel.IsVisible, Is.True);
        }

        [Test]
        public void Answering_no_closes_the_popup_and_leaves_gameplay_active()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            Popup.No();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(_fakeFactory.ReturnedWindows.Contains(Popup), Is.True);
            Assert.That(Panel.IsVisible, Is.True);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void Answering_yes_enters_the_menu_state()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            Popup.Yes();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(MainMenuState)));
            Assert.That(_fakeStateMachine.ChangeStateCount, Is.EqualTo(1));
        }

        [Test]
        public void Answering_yes_leaves_gameplay_so_its_popup_and_panel_are_cleaned_up_on_the_way_out()
        {
            _gameplayState.Enter();
            _gameplayState.Back();

            Popup.Yes();
            _gameplayState.Leave();

            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(Popup));
            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(Panel));
            Assert.That(_fakeInputService.IsPlayerPressEnabled, Is.True);
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
