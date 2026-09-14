using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class MainMenuStateTests
    {
        private List<string> _log;
        private FakeUIService _fakeUIService;
        private MainMenuState _mainMenuState;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _fakeUIService = new FakeUIService();
            _mainMenuState = new MainMenuState(new RecordingBoardSession(_log), new RecordingStateMachine(_log), _fakeUIService);
        }

        [Test]
        public void Starting_the_game_resets_the_board_session_before_entering_gameplay()
        {
            _mainMenuState.Enter();

            _fakeUIService.LastMainMenuPanel.StartGame();

            Assert.That(_log, Is.EqualTo(new[]
            {
                "BoardSession.Reset",
                "StateMachine.ChangeState<GameplayState>",
            }));
        }

        [Test]
        public void Back_on_the_menu_changes_no_state_or_window()
        {
            _mainMenuState.Enter();

            _mainMenuState.Back();

            Assert.That(_log, Is.Empty);
            Assert.That(_fakeUIService.HasPanel, Is.True);
            Assert.That(_fakeUIService.HasPopup, Is.False);
        }

        [Test]
        public void Leaving_the_menu_closes_its_panel()
        {
            _mainMenuState.Enter();

            _mainMenuState.Leave();

            Assert.That(_fakeUIService.HasPanel, Is.False);
        }
    }
}
