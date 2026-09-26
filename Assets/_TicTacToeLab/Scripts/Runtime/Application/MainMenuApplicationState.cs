namespace TicTacToeLab.Runtime
{
    internal sealed class MainMenuApplicationState : IApplicationState
    {
        private readonly ApplicationFlow _flow;
        private readonly ApplicationStateFactory _stateFactory;
        private readonly IApplicationUI _applicationUI;
        private readonly IInputService _inputService;

        public MainMenuApplicationState(
            ApplicationFlow flow,
            ApplicationStateFactory stateFactory,
            IApplicationUI applicationUI,
            IInputService inputService)
        {
            _flow = flow;
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
            _flow.TransitionFrom(this, _stateFactory.CreateGameplayState());
        }
    }
}
