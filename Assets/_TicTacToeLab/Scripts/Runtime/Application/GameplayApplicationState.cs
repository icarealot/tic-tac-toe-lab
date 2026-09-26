namespace TicTacToeLab.Runtime
{
    internal sealed class GameplayApplicationState : IApplicationState
    {
        private readonly ApplicationFlow _flow;
        private readonly ApplicationStateFactory _stateFactory;
        private readonly BoardPresenter _boardPresenter;
        private readonly IApplicationUI _applicationUI;
        private readonly IInputService _inputService;

        private bool _isActive;
        private QuitConfirmation _activeQuitConfirmation;

        public GameplayApplicationState(
            ApplicationFlow flow,
            ApplicationStateFactory stateFactory,
            BoardPresenter boardPresenter,
            IApplicationUI applicationUI,
            IInputService inputService)
        {
            _flow = flow;
            _stateFactory = stateFactory;
            _boardPresenter = boardPresenter;
            _applicationUI = applicationUI;
            _inputService = inputService;
        }

        public void Enter()
        {
            _isActive = true;
            _boardPresenter.Reset();
            _inputService.BackPressed += HandleBack;
            _boardPresenter.GameEnded += HandleGameEnded;
            _applicationUI.ShowGameplay(_boardPresenter, HandleBack);
            _inputService.EnablePlayerPress();
        }

        public void Exit()
        {
            _isActive = false;
            _activeQuitConfirmation = null;
            _inputService.BackPressed -= HandleBack;
            _boardPresenter.GameEnded -= HandleGameEnded;
        }

        private void HandleBack()
        {
            if (!_isActive)
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

        private void HandleGameEnded()
        {
            if (!_isActive)
            {
                return;
            }

            Outcome outcome = _boardPresenter.Outcome;
            _flow.TransitionFrom(this, _stateFactory.CreateOutcomeState(outcome));
        }

        private void OpenQuitConfirmation()
        {
            QuitConfirmation quitConfirmation = new(this);
            _activeQuitConfirmation = quitConfirmation;
            _inputService.DisablePlayerPress();
            _applicationUI.ShowQuitConfirmation(quitConfirmation.Confirm, quitConfirmation.Close);
        }

        private void ConfirmQuit(QuitConfirmation quitConfirmation)
        {
            if (!IsActiveQuitConfirmation(quitConfirmation))
            {
                return;
            }

            _flow.TransitionFrom(this, _stateFactory.CreateMainMenuState());
        }

        private void CloseQuitConfirmation(QuitConfirmation quitConfirmation)
        {
            if (!IsActiveQuitConfirmation(quitConfirmation))
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

        private bool IsActiveQuitConfirmation(QuitConfirmation quitConfirmation)
        {
            return _isActive && _activeQuitConfirmation == quitConfirmation;
        }

        private sealed class QuitConfirmation
        {
            private readonly GameplayApplicationState _gameplayState;

            public QuitConfirmation(GameplayApplicationState gameplayState)
            {
                _gameplayState = gameplayState;
            }

            public void Confirm()
            {
                _gameplayState.ConfirmQuit(this);
            }

            public void Close()
            {
                _gameplayState.CloseQuitConfirmation(this);
            }
        }
    }
}
