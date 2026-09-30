namespace TicTacToeLab.Runtime
{
    public sealed class PvpTurnController : IGameplayTurnController
    {
        private readonly IInputService _inputService;
        private TurnLifecycle _lifecycle;

        public PvpTurnController(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void Enter()
        {
            if (_lifecycle != TurnLifecycle.Constructed)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Active;
            _inputService.EnablePlayerPress();
        }

        public void Pause()
        {
            if (_lifecycle != TurnLifecycle.Active)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Paused;
            _inputService.DisablePlayerPress();
        }

        public void Resume()
        {
            if (_lifecycle != TurnLifecycle.Paused)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Active;
            _inputService.EnablePlayerPress();
        }

        public void Exit()
        {
            if (_lifecycle == TurnLifecycle.Exited)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Exited;
            _inputService.DisablePlayerPress();
        }
    }
}
