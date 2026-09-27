namespace TicTacToeLab.Runtime
{
    internal sealed class GameplayAppState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly BoardPresenter _boardPresenter;
        private readonly IAppUI _appUI;
        private readonly IInputService _inputService;

        private bool _isActive;
        private QuitConfirmation _activeQuitConfirmation;

        public GameplayAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            BoardPresenter boardPresenter,
            IAppUI appUI,
            IInputService inputService)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _boardPresenter = boardPresenter;
            _appUI = appUI;
            _inputService = inputService;
        }

        public void Enter()
        {
            _isActive = true;
            _boardPresenter.Reset();
            _inputService.BackPressed += HandleBack;
            _boardPresenter.GameEnded += HandleGameEnded;
            _appUI.ShowGameplay(_boardPresenter, HandleBack);
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
            _stateMachine.TransitionFrom(this, _stateFactory.CreateOutcomeState(outcome));
        }

        private void OpenQuitConfirmation()
        {
            QuitConfirmation quitConfirmation = new(this);
            _activeQuitConfirmation = quitConfirmation;
            _inputService.DisablePlayerPress();
            _appUI.ShowQuitConfirmation(quitConfirmation.Confirm, quitConfirmation.Close);
        }

        private void ConfirmQuit(QuitConfirmation quitConfirmation)
        {
            if (!IsActiveQuitConfirmation(quitConfirmation))
            {
                return;
            }

            _stateMachine.TransitionFrom(this, _stateFactory.CreateMainMenuState());
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
            _appUI.CloseQuitConfirmation();
            _inputService.EnablePlayerPress();
        }

        private bool IsActiveQuitConfirmation(QuitConfirmation quitConfirmation)
        {
            return _isActive && _activeQuitConfirmation == quitConfirmation;
        }

        private sealed class QuitConfirmation
        {
            private readonly GameplayAppState _gameplayState;

            public QuitConfirmation(GameplayAppState gameplayState)
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
