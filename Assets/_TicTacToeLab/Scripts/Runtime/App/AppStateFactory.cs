namespace TicTacToeLab.Runtime
{
    internal sealed class AppStateFactory
    {
        private readonly AppStateMachine _stateMachine;
        private readonly BoardPresenter _boardPresenter;
        private readonly IAppUI _appUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;

        public AppStateFactory(
            AppStateMachine stateMachine,
            BoardPresenter boardPresenter,
            IAppUI appUI,
            IDelayScheduler delayScheduler,
            IInputService inputService)
        {
            _stateMachine = stateMachine;
            _boardPresenter = boardPresenter;
            _appUI = appUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
        }

        public IAppState CreateHomeState()
        {
            return new HomeAppState(_stateMachine, this, _appUI, _inputService);
        }

        public IAppState CreateGameplayState()
        {
            return new GameplayAppState(_stateMachine, this, _boardPresenter, _appUI, _inputService);
        }

        public IAppState CreateOutcomeState(Outcome outcome)
        {
            return new OutcomeAppState(_stateMachine, this, _appUI, _delayScheduler, _inputService, outcome);
        }
    }
}
