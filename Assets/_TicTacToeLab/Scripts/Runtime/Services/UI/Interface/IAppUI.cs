using System;

namespace TicTacToeLab.Runtime
{
    public interface IAppUI
    {
        public void Show<TScreen>(Action<TScreen> configure = null) where TScreen : IScreen;
        public void Close<TScreen>() where TScreen : IScreen;
    }
}
