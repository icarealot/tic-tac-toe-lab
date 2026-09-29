using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeHomeScreen : FakeScreen, IHomeScreen
    {
        public Action OnPvp { get; private set; }
        public Action OnPve { get; private set; }

        public void Setup(Action onPvp, Action onPve)
        {
            OnPvp = onPvp;
            OnPve = onPve;
        }

        public void ClickPvp()
        {
            OnPvp?.Invoke();
        }

        public void ClickPve()
        {
            OnPve?.Invoke();
        }
    }
}
