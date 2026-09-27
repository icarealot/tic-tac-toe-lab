namespace TicTacToeLab.Runtime
{
    internal sealed class AppStateFactory
    {
        private readonly AppStateMachine _stateMachine;
        private readonly BoardPresenter _boardPresenter;
        private readonly IApplicationUI _applicationUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;

        public AppStateFactory(
            AppStateMachine stateMachine,
            BoardPresenter boardPresenter,
            IApplicationUI applicationUI,
            IDelayScheduler delayScheduler,
            IInputService inputService)
        {
            _stateMachine = stateMachine;
            _boardPresenter = boardPresenter;
            _applicationUI = applicationUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
        }

        public IAppState CreateMainMenuState()
        {
            return new MainMenuAppState(_stateMachine, this, _applicationUI, _inputService);
        }

        public IAppState CreateGameplayState()
        {
            return new GameplayAppState(_stateMachine, this, _boardPresenter, _applicationUI, _inputService);
        }

        public IAppState CreateOutcomeState(Outcome outcome)
        {
            return new OutcomeAppState(_stateMachine, this, _applicationUI, _delayScheduler, _inputService, outcome);
        }
    }
}
