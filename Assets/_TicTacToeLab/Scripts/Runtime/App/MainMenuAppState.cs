namespace TicTacToeLab.Runtime
{
    internal sealed class MainMenuAppState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly IApplicationUI _applicationUI;
        private readonly IInputService _inputService;

        public MainMenuAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            IApplicationUI applicationUI,
            IInputService inputService)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _applicationUI = applicationUI;
            _inputService = inputService;
        }

        public void Enter()
        {
            _inputService.DisablePlayerPress();
            _applicationUI.ClosePopup();
            _applicationUI.ShowMainMenu(StartGame);
        }

        public void Exit()
        {
        }

        private void StartGame()
        {
            _stateMachine.TransitionFrom(this, _stateFactory.CreateGameplayState());
        }
    }
}
