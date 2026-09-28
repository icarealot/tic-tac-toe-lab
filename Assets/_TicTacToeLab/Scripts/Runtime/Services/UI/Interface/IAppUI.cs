using System;

namespace TicTacToeLab.Runtime
{
    public interface IAppUI
    {
        public void Show<TScreen>(Action<TScreen> configure = null) where TScreen : IScreen;
        public void Close<TScreen>() where TScreen : IScreen;
        public void ShowHome(Action onStart);
        public void ShowGameplay(BoardPresenter boardPresenter, Action onBack);
        public void ShowQuitConfirmation(Action onQuit, Action onCancel);
        public void CloseQuitConfirmation();
        public void ShowOutcome(Outcome outcome, Action onContinue);
        public void ClosePopup();
    }
}
