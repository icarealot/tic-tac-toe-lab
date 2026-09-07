namespace TicTacToeLab.Runtime
{
    public sealed class GameplayState : IAppState
    {
        private readonly IBoardSession _boardSession;
        private readonly IStateMachine _stateMachine;
        private readonly IUIService _uiService;
        private readonly IInputService _inputService;
        private readonly ILogService _logService;

        public GameplayState(IBoardSession boardSession, IStateMachine stateMachine, IUIService uiService, IInputService inputService, ILogService logService)
        {
            _boardSession = boardSession;
            _stateMachine = stateMachine;
            _uiService = uiService;
            _inputService = inputService;
            _logService = logService;
        }

        public void Enter()
        {
            _boardSession.GameEnded += OnGameEnded;
            _uiService.ShowPanel<IGameplayPanel>(panel => panel.Setup(_boardSession));
        }

        public void Leave()
        {
            _boardSession.GameEnded -= OnGameEnded;

            CloseConfirmQuitPopup();
            _ = _uiService.TryClosePanel();
        }

        public void Back()
        {
            if (_uiService.HasPopup)
            {
                CloseConfirmQuitPopup();
                return;
            }

            OpenConfirmQuitPopup();
        }

        private void OpenConfirmQuitPopup()
        {
            _inputService.DisablePlayerPress();
            _uiService.ShowPopup<IConfirmQuitPopup>(popup =>
            {
                popup.Setup(
                    onYes: Quit,
                    onNo: CloseConfirmQuitPopup);
            });
        }

        private void CloseConfirmQuitPopup()
        {
            if (!_uiService.HasPopup)
            {
                return;
            }

            _uiService.CloseAllPopups();
            _inputService.EnablePlayerPress();
        }

        private void Quit()
        {
            _logService.Log("Quitting the game");
        }

        private void OnGameEnded()
        {
            _stateMachine.ChangeState<GameCompleteState>();
        }
    }
}
