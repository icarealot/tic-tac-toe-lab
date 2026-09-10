namespace TicTacToeLab.Runtime
{
    public sealed class MainMenuState : IAppState
    {
        private readonly IBoardSession _boardSession;
        private readonly IStateMachine _stateMachine;
        private readonly IUIService _uiService;

        public MainMenuState(
            IBoardSession boardSession,
            IStateMachine stateMachine,
            IUIService uiService)
        {
            _boardSession = boardSession;
            _stateMachine = stateMachine;
            _uiService = uiService;
        }

        public void Enter()
        {
            _uiService.ShowPanel<IMainMenuPanel>(panel => panel.Setup(onStart: StartGame));
        }

        public void Leave()
        {
            _ = _uiService.TryClosePanel();
        }

        public void Back() { }

        private void StartGame()
        {
            _boardSession.Reset();
            _stateMachine.ChangeState<GameplayState>();
        }
    }
}
