namespace TicTacToeLab.Runtime
{
    internal sealed class GameplayAppState : IAppState
    {
        private const float BOT_TURN_MINIMUM_DELAY_SECONDS = 0.4f;
        private const float BOT_TURN_MAXIMUM_DELAY_SECONDS = 1f;
        private static readonly float[] _botTurnDelaysSeconds =
        {
            BOT_TURN_MINIMUM_DELAY_SECONDS,
            0.5f,
            0.6f,
            0.7f,
            0.8f,
            0.9f,
            BOT_TURN_MAXIMUM_DELAY_SECONDS,
        };

        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly BoardPresenter _boardPresenter;
        private readonly IAppUI _appUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;
        private readonly GameSetup _setup;
        private readonly IBot _bot;
        private readonly IRandomChoiceSource _randomChoiceSource;

        private bool _isActive;
        private bool _botPlacementPending;
        private QuitConfirmation _activeQuitConfirmation;

        public GameplayAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            BoardPresenter boardPresenter,
            IAppUI appUI,
            IDelayScheduler delayScheduler,
            IInputService inputService,
            GameSetup setup,
            IBot bot,
            IRandomChoiceSource randomChoiceSource)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _boardPresenter = boardPresenter;
            _appUI = appUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
            _setup = setup;
            _bot = bot;
            _randomChoiceSource = randomChoiceSource;
        }

        public void Enter()
        {
            _isActive = true;
            _boardPresenter.Reset();
            _inputService.BackPressed += HandleBack;
            _boardPresenter.GameEnded += HandleGameEnded;

            if (_bot != null)
            {
                _boardPresenter.TurnChanged += HandleTurnChanged;
            }

            _appUI.Show<IGameplayScreen>(screen => screen.Setup(_boardPresenter, _setup, HandleBack));
            _inputService.EnablePlayerPress();
        }

        public void Exit()
        {
            _isActive = false;
            _activeQuitConfirmation = null;
            _inputService.BackPressed -= HandleBack;
            _boardPresenter.GameEnded -= HandleGameEnded;

            if (_bot != null)
            {
                _boardPresenter.TurnChanged -= HandleTurnChanged;
            }
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

        private void HandleTurnChanged(Mark turn)
        {
            if (!_isActive || _setup.Mode != GameMode.Pve)
            {
                return;
            }

            if (turn == Mark.O)
            {
                ScheduleBotPlacement();
                return;
            }

            _inputService.EnablePlayerPress();
        }

        private void ScheduleBotPlacement()
        {
            if (_botPlacementPending)
            {
                return;
            }

            _botPlacementPending = true;
            _inputService.DisablePlayerPress();
            int delayIndex = _randomChoiceSource.NextIndex(_botTurnDelaysSeconds.Length);
            float delaySeconds = _botTurnDelaysSeconds[delayIndex];
            _ = _delayScheduler.Schedule(delaySeconds, PlaceBotMark);
        }

        private void PlaceBotMark()
        {
            _botPlacementPending = false;

            if (!_isActive
                || _setup.Mode != GameMode.Pve
                || _bot == null
                || _boardPresenter.Outcome != Outcome.InProgress
                || _boardPresenter.Turn != Mark.O)
            {
                return;
            }

            CellCoordinate coordinate = _boardPresenter.SelectBotPlacement(_bot);
            _ = _boardPresenter.TryPlaceMark(coordinate);
        }

        private void OpenQuitConfirmation()
        {
            QuitConfirmation quitConfirmation = new(this);
            _activeQuitConfirmation = quitConfirmation;
            _inputService.DisablePlayerPress();
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
