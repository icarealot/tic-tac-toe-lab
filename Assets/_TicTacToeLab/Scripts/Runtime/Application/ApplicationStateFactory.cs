namespace TicTacToeLab.Runtime
{
    internal sealed class ApplicationStateFactory
    {
        private readonly ApplicationFlow _flow;
        private readonly BoardPresenter _boardPresenter;
        private readonly IApplicationUI _applicationUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;

        public ApplicationStateFactory(
            ApplicationFlow flow,
            BoardPresenter boardPresenter,
            IApplicationUI applicationUI,
            IDelayScheduler delayScheduler,
            IInputService inputService)
        {
            _flow = flow;
            _boardPresenter = boardPresenter;
            _applicationUI = applicationUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
        }

        public IApplicationState CreateMainMenuState()
        {
            return new MainMenuApplicationState(_flow, this, _applicationUI, _inputService);
        }

        public IApplicationState CreateGameplayState()
        {
            return new GameplayApplicationState(_flow, this, _boardPresenter, _applicationUI, _inputService);
        }

        public IApplicationState CreateOutcomeState(Outcome outcome)
        {
            return new OutcomeApplicationState(_flow, this, _applicationUI, _delayScheduler, _inputService, outcome);
        }
    }
}
