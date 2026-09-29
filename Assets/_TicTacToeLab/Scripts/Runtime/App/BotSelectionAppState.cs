namespace TicTacToeLab.Runtime
{
    internal sealed class BotSelectionAppState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly IAppUI _appUI;
        private readonly IInputService _inputService;

        private bool _isActive;

        public BotSelectionAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            IAppUI appUI,
            IInputService inputService)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _appUI = appUI;
            _inputService = inputService;
        }

        public void Enter()
        {
            _isActive = true;
            _inputService.DisablePlayerPress();
            _inputService.BackPressed += HandleBack;
            _appUI.Show<IBotSelectionScreen>(screen => screen.Setup(
                StartAmateurGame,
                StartProfessionalGame,
                ReturnHome));
        }

        public void Exit()
        {
            _isActive = false;
            _inputService.BackPressed -= HandleBack;
        }

        private void StartAmateurGame()
        {
            StartGame(BotDifficulty.Amateur);
        }

        private void StartProfessionalGame()
        {
            StartGame(BotDifficulty.Professional);
        }

        private void StartGame(BotDifficulty botDifficulty)
        {
            if (!_isActive)
            {
                return;
            }

            _stateMachine.TransitionFrom(this, _stateFactory.CreateGameplayState(new GameSetup(GameMode.Pve, botDifficulty)));
        }

        private void ReturnHome()
        {
            if (!_isActive)
            {
                return;
            }

            _stateMachine.TransitionFrom(this, _stateFactory.CreateHomeState());
        }

        private void HandleBack()
        {
            ReturnHome();
        }
    }
}
