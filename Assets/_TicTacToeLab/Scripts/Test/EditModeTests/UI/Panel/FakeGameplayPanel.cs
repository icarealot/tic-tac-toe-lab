using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeGameplayPanel : FakeWindow, IGameplayPanel
    {
        public IBoardSession ConfiguredBoardSession { get; private set; }
        public Action ConfiguredOnBack { get; private set; }

        private Action _onBack;

        public void Setup(IBoardSession boardSession, Action onBack)
        {
            ConfiguredBoardSession = boardSession;
            _onBack = onBack;
            ConfiguredOnBack = onBack;
        }

        public void Back()
        {
            _onBack?.Invoke();
        }
    }
}
