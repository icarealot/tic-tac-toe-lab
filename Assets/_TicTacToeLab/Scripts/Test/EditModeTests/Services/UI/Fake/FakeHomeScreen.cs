using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeHomeScreen : FakeScreen, IHomeScreen
    {
        public Action OnStart { get; private set; }

        public void Setup(Action onStart)
        {
            OnStart = onStart;
        }

        public void ClickStart()
        {
            OnStart?.Invoke();
        }
    }
}
