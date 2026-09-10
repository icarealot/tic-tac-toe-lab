using System;

namespace TicTacToeLab.Runtime
{
    public interface IMainMenuPanel : IPanel
    {
        public void Setup(Action onStart);
    }
}
