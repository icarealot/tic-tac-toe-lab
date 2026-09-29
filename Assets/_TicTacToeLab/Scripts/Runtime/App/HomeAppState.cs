namespace TicTacToeLab.Runtime
{
    internal sealed class HomeAppState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly IAppUI _appUI;
        private readonly IInputService _inputService;

        public HomeAppState(
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
            _inputService.DisablePlayerPress();
            _appUI.Show<IHomeScreen>(screen => screen.Setup(StartPvp, StartPve));
        }

        public void Exit()
        {
        }

        private void StartPvp()
        {
            _stateMachine.TransitionFrom(this, _stateFactory.CreateGameplayState(new GameSetup(GameMode.Pvp, null)));
        }

        private void StartPve()
        {
            _stateMachine.TransitionFrom(this, _stateFactory.CreateBotSelectionState());
        }
    }
}
