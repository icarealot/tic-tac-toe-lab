using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeMainMenuPanel : FakeWindow, IMainMenuPanel
    {
        private Action _onStart;

        public Action ConfiguredOnStart { get; private set; }

        public void Setup(Action onStart)
        {
            _onStart = onStart;
            ConfiguredOnStart = onStart;
        }

        public void StartGame()
        {
            _onStart?.Invoke();
        }
    }
}
