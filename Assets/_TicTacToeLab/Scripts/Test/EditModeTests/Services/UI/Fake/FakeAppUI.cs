using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeAppUI : IAppUI
    {
        public bool IsMainMenuVisible { get; private set; }
        public bool IsGameplayVisible { get; private set; }
        public bool IsQuitConfirmationVisible { get; private set; }
        public bool IsOutcomeVisible { get; private set; }
        public bool HasPopup => IsQuitConfirmationVisible || IsOutcomeVisible;

        public int MainMenuPresentationCount { get; private set; }
        public int OutcomePresentationCount { get; private set; }
        public Outcome ShownOutcome { get; private set; }
        public Outcome OutcomeWhenGameplayShown { get; private set; }

        public Action QuitConfirmationOnConfirm { get; private set; }
        public Action QuitConfirmationOnCancel { get; private set; }

        private Action _onStart;
        private Action _onBack;
        private Action _onContinue;

        public void ShowMainMenu(Action onStart)
        {
            MainMenuPresentationCount++;
            IsMainMenuVisible = true;
            IsGameplayVisible = false;
            _onStart = onStart;
        }

        public void ShowGameplay(BoardPresenter boardPresenter, Action onBack)
        {
            IsGameplayVisible = true;
            IsMainMenuVisible = false;
            OutcomeWhenGameplayShown = boardPresenter.Outcome;
            _onBack = onBack;
        }

        public void ShowQuitConfirmation(Action onQuit, Action onCancel)
        {
            IsQuitConfirmationVisible = true;
            IsOutcomeVisible = false;
            QuitConfirmationOnConfirm = onQuit;
            QuitConfirmationOnCancel = onCancel;
        }

        public void CloseQuitConfirmation()
        {
            IsQuitConfirmationVisible = false;
        }

        public void ShowOutcome(Outcome outcome, Action onContinue)
        {
            OutcomePresentationCount++;
            IsOutcomeVisible = true;
            IsQuitConfirmationVisible = false;
            ShownOutcome = outcome;
            _onContinue = onContinue;
        }

        public void ClosePopup()
        {
            IsQuitConfirmationVisible = false;
            IsOutcomeVisible = false;
        }

        public void ClickStart()
        {
            _onStart?.Invoke();
        }

        public void ClickGameplayBack()
        {
            _onBack?.Invoke();
        }

        public void ClickYes()
        {
            QuitConfirmationOnConfirm?.Invoke();
        }

        public void ClickNo()
        {
            QuitConfirmationOnCancel?.Invoke();
        }

        public void ClickContinue()
        {
            _onContinue?.Invoke();
        }
    }
}
