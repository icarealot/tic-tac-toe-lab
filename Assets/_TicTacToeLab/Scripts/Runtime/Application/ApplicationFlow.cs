using System;

namespace TicTacToeLab.Runtime
{
    public sealed class ApplicationFlow : IDisposable
    {
        private const float OUTCOME_PRESENTATION_DELAY_SECONDS = 1f;

        private enum Phase
        {
            MainMenu,
            Gameplay,
            OutcomePending,
            OutcomePresented,
            Disposed
        }

        private readonly BoardPresenter _boardPresenter;
        private readonly IApplicationUI _applicationUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;

        private Phase _phase = Phase.MainMenu;
        private QuitConfirmation _activeQuitConfirmation;
        private IDisposable _pendingOutcomePresentation;
        private Outcome _capturedOutcome;

        public ApplicationFlow(
            BoardPresenter boardPresenter,
            IApplicationUI applicationUI,
            IDelayScheduler delayScheduler,
            IInputService inputService)
        {
            _boardPresenter = boardPresenter;
            _applicationUI = applicationUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;

            _boardPresenter.GameEnded += OnGameEnded;
            _inputService.BackPressed += OnBackPressed;
        }

        public void Start()
        {
            if (_phase == Phase.Disposed)
            {
                return;
            }

            EnterMainMenu();
        }

        public void Dispose()
        {
            if (_phase == Phase.Disposed)
            {
                return;
            }

            _phase = Phase.Disposed;
            _activeQuitConfirmation = null;
            CancelPendingOutcomePresentation();
            _boardPresenter.GameEnded -= OnGameEnded;
            _inputService.BackPressed -= OnBackPressed;
        }

        private void OnBackPressed()
        {
            switch (_phase)
            {
                case Phase.Gameplay:
                    HandleGameplayBack();
                    break;
                case Phase.OutcomePresented:
                    AcknowledgeOutcome();
                    break;
            }
        }

        private void OnGameEnded()
        {
            if (_phase != Phase.Gameplay)
            {
                return;
            }

            _phase = Phase.OutcomePending;
            _inputService.DisablePlayerPress();
            _capturedOutcome = _boardPresenter.Outcome;
            _pendingOutcomePresentation = _delayScheduler.Schedule(OUTCOME_PRESENTATION_DELAY_SECONDS, PresentOutcome);
        }

        private void PresentOutcome()
        {
            if (_phase != Phase.OutcomePending)
            {
                return;
            }

            _pendingOutcomePresentation = null;
            _phase = Phase.OutcomePresented;
            _applicationUI.ShowOutcome(_capturedOutcome, AcknowledgeOutcome);
        }

        private void AcknowledgeOutcome()
        {
            if (_phase != Phase.OutcomePresented)
            {
                return;
            }

            EnterMainMenu();
        }

        private void CancelPendingOutcomePresentation()
        {
            _pendingOutcomePresentation?.Dispose();
            _pendingOutcomePresentation = null;
        }

        private void EnterMainMenu()
        {
            _phase = Phase.MainMenu;
            _activeQuitConfirmation = null;
            _inputService.DisablePlayerPress();
            _applicationUI.ClosePopup();
            _applicationUI.ShowMainMenu(StartGame);
        }

        private void StartGame()
        {
            if (_phase != Phase.MainMenu)
            {
                return;
            }

            _phase = Phase.Gameplay;
            _boardPresenter.Reset();
            _applicationUI.ShowGameplay(_boardPresenter, HandleGameplayBack);
            _inputService.EnablePlayerPress();
        }

        private void HandleGameplayBack()
        {
            if (_phase != Phase.Gameplay)
            {
                return;
            }

            if (_activeQuitConfirmation != null)
            {
                CloseActiveQuitConfirmation();
                return;
            }

            OpenQuitConfirmation();
        }

        private void OpenQuitConfirmation()
        {
            QuitConfirmation confirmation = new(this);
            _activeQuitConfirmation = confirmation;
            _inputService.DisablePlayerPress();
            _applicationUI.ShowQuitConfirmation(confirmation.Confirm, confirmation.Close);
        }

        private void CloseQuitConfirmation(QuitConfirmation confirmation)
        {
            if (!IsActiveQuitConfirmation(confirmation))
            {
                return;
            }

            CloseActiveQuitConfirmation();
        }

        private void CloseActiveQuitConfirmation()
        {
            _activeQuitConfirmation = null;
            _applicationUI.CloseQuitConfirmation();
            _inputService.EnablePlayerPress();
        }

        private bool IsActiveQuitConfirmation(QuitConfirmation confirmation)
        {
            return _phase == Phase.Gameplay && _activeQuitConfirmation == confirmation;
        }

        private void ConfirmQuit(QuitConfirmation confirmation)
        {
            if (!IsActiveQuitConfirmation(confirmation))
            {
                return;
            }

            _activeQuitConfirmation = null;
            EnterMainMenu();
        }

        private sealed class QuitConfirmation
        {
            private readonly ApplicationFlow _flow;

            public QuitConfirmation(ApplicationFlow flow)
            {
                _flow = flow;
            }

            public void Confirm()
            {
                _flow.ConfirmQuit(this);
            }

            public void Close()
            {
                _flow.CloseQuitConfirmation(this);
            }
        }
    }
}
