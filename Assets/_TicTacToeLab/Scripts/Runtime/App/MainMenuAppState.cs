namespace TicTacToeLab.Runtime
{
    internal sealed class MainMenuAppState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly IAppUI _appUI;
        private readonly IInputService _inputService;

        public MainMenuAppState(
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
            _appUI.ClosePopup();
            _appUI.ShowMainMenu(StartGame);
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
