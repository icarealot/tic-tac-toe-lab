using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class MainMenuStateTests
    {
        private FakeMainMenuPanel Panel => _fakeFactory.MenuPanels[0];

        private FakeBoardSession _fakeBoardSession;
        private FakeFactoryService _fakeFactory;
        private UIService _uiService;
        private FakeStateMachine _fakeStateMachine;
        private MainMenuState _mainMenuState;

        [SetUp]
        public void SetUp()
        {
            _fakeBoardSession = new FakeBoardSession();
            _fakeFactory = new FakeFactoryService();
            _uiService = new UIService(_fakeFactory,
                                        new FakeUIRoot(),
                                        new FakeCoroutineService());
            _fakeStateMachine = new FakeStateMachine();
            _mainMenuState = new MainMenuState(_fakeBoardSession,
                                            _fakeStateMachine,
                                            _uiService);
        }

        [Test]
        public void Entering_the_menu_asks_for_the_menu_panel_by_interface_and_configures_it()
        {
            _mainMenuState.Enter();

            Assert.That(_fakeFactory.MenuPanels, Has.Count.EqualTo(1));
            Assert.That(Panel.IsVisible, Is.True);
            Assert.That(Panel.ConfiguredOnStart, Is.Not.Null);
        }

        [Test]
        public void Leaving_the_menu_closes_the_menu_panel()
        {
            _mainMenuState.Enter();

            _mainMenuState.Leave();

            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(Panel));
        }

        [Test]
        public void Back_on_the_menu_does_nothing()
        {
            _mainMenuState.Enter();

            _mainMenuState.Back();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(_fakeFactory.Popups, Has.Count.EqualTo(0));
            Assert.That(_fakeFactory.ReturnedWindows, Has.Count.EqualTo(0));
            Assert.That(Panel.IsVisible, Is.True);
            Assert.That(_fakeStateMachine.ChangedStateType, Is.Null);
        }

        [Test]
        public void A_start_press_resets_the_board_session()
        {
            _mainMenuState.Enter();

            Panel.StartGame();

            Assert.That(_fakeBoardSession.WasReset, Is.True);
            Assert.That(_fakeBoardSession.ResetCount, Is.EqualTo(1));
        }

        [Test]
        public void A_start_press_enters_the_gameplay_state()
        {
            _mainMenuState.Enter();

            Panel.StartGame();

            Assert.That(_fakeStateMachine.ChangedStateType, Is.EqualTo(typeof(GameplayState)));
            Assert.That(_fakeStateMachine.ChangeStateCount, Is.EqualTo(1));
        }

        [Test]
        public void A_start_press_leaves_the_menu_so_its_panel_is_closed()
        {
            _mainMenuState.Enter();

            Panel.StartGame();
            _mainMenuState.Leave();

            Assert.That(_fakeFactory.ReturnedWindows, Does.Contain(Panel));
        }
    }
}
