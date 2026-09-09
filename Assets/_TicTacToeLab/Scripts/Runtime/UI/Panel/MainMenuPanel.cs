using System;

namespace TicTacToeLab.Runtime
{
    public class MainMenuPanel : Panel, IMainMenuPanel
    {
        private Action _onStart;

        public void Setup(Action onStart)
        {
            _onStart = onStart;
        }

        public void StartGame()
        {
            _onStart?.Invoke();
        }
    }
}
