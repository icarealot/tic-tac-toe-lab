namespace TicTacToeLab.Runtime
{
    internal sealed class GameplayAppState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly BoardPresenter _boardPresenter;
        private readonly IAppUI _appUI;
        private readonly IInputService _inputService;
        private readonly GameSetup _setup;
        private readonly IGameplayTurnController _turnController;

        private bool _isActive;
        private QuitConfirmation _activeQuitConfirmation;

        public GameplayAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            BoardPresenter boardPresenter,
            IAppUI appUI,
            IInputService inputService,
            GameSetup setup,
            IGameplayTurnController turnController)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _boardPresenter = boardPresenter;
            _appUI = appUI;
            _inputService = inputService;
            _setup = setup;
            _turnController = turnController;
        }

        public void Enter()
        {
            _isActive = true;
            _boardPresenter.Reset();
            _inputService.BackPressed += HandleBack;
            _boardPresenter.GameEnded += HandleGameEnded;
            _appUI.Show<IGameplayScreen>(screen => screen.Setup(_boardPresenter, _setup, HandleBack));
            _turnController.Enter();
        }

        public void Exit()
        {
            _isActive = false;
            _turnController.Exit();
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
            _stateMachine.TransitionFrom(this, _stateFactory.CreateOutcomeState(outcome, _setup));
        }

        private void OpenQuitConfirmation()
        {
            _turnController.Pause();
            QuitConfirmation quitConfirmation = new(this);
            _activeQuitConfirmation = quitConfirmation;
            _appUI.Show<IConfirmQuitScreen>(screen => screen.Setup(quitConfirmation.Confirm, quitConfirmation.Close));
        }

        private void ConfirmQuit(QuitConfirmation quitConfirmation)
        {
            if (!IsActiveQuitConfirmation(quitConfirmation))
            {
                return;
            }

            _activeQuitConfirmation = null;
            _appUI.Close<IConfirmQuitScreen>();
            _stateMachine.TransitionFrom(this, _stateFactory.CreateHomeState());
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
            _appUI.Close<IConfirmQuitScreen>();
            _turnController.Resume();
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
