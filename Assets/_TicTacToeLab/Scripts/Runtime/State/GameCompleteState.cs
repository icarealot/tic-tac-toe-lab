namespace TicTacToeLab.Runtime
{
    public sealed class GameCompleteState : IAppState
    {
        public const float RESET_PAUSE_SECONDS = 1f;

        private readonly IBoardSession _boardSession;
        private readonly IStateMachine _stateMachine;
        private readonly ICoroutineService _coroutineService;

        private CoroutineHandle _pendingReset;

        public GameCompleteState(IBoardSession boardSession, IStateMachine stateMachine, ICoroutineService coroutineService)
        {
            _boardSession = boardSession;
            _stateMachine = stateMachine;
            _coroutineService = coroutineService;
        }

        public void Enter()
        {
            _pendingReset = _coroutineService.RunAfter(RESET_PAUSE_SECONDS, ResetAfterPause);
        }

        public void Leave()
        {
            _pendingReset?.Dispose();
            _pendingReset = null;
        }

        private void ResetAfterPause()
        {
            _boardSession.Reset();
            _stateMachine.ChangeState<GameplayState>();
        }
    }
}
