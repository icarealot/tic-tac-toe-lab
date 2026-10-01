namespace TicTacToeLab.Runtime
{
    public sealed class PvpTurnController : IGameplayTurnController
    {
        private readonly IInputService _inputService;

        public PvpTurnController(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void Enter()
        {
            _inputService.EnablePlayerPress();
        }

        public void Pause()
        {
            _inputService.DisablePlayerPress();
        }

        public void Resume()
        {
            _inputService.EnablePlayerPress();
        }

        public void Exit()
        {
            _inputService.DisablePlayerPress();
        }
    }
}
