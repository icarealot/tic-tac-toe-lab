using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeGameplayScreen : FakeScreen, IGameplayScreen
    {
        public Outcome BoardOutcomeWhenPresented { get; private set; }
        public Action OnBack { get; private set; }

        public void Setup(BoardPresenter boardPresenter, Action onBack)
        {
            BoardOutcomeWhenPresented = boardPresenter.Outcome;
            OnBack = onBack;
        }

        public void ClickBack()
        {
            OnBack?.Invoke();
        }
    }
}
