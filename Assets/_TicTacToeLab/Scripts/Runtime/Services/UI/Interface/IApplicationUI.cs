using System;

namespace TicTacToeLab.Runtime
{
    public interface IApplicationUI
    {
        public void ShowMainMenu(Action onStart);
        public void ShowGameplay(BoardPresenter boardPresenter, Action onBack);
        public void ShowQuitConfirmation(Action onQuit, Action onCancel);
        public void CloseQuitConfirmation();
        public void ShowOutcome(Outcome outcome, Action onContinue);
        public void ClosePopup();
    }
}
