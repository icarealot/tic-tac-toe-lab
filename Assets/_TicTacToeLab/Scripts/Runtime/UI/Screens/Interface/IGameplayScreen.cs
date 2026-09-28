using System;

namespace TicTacToeLab.Runtime
{
    public interface IGameplayScreen : IScreen
    {
        public void Setup(BoardPresenter boardPresenter, Action onBack);
    }
}
