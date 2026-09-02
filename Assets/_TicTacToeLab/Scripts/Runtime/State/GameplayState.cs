namespace TicTacToeLab.Runtime
{
    public sealed class GameplayState : IAppState
    {
        private readonly BoardSession _boardSession;
        private readonly AppStateMachine _stateMachine;

        public GameplayState(BoardSession boardSession, AppStateMachine stateMachine)
        {
            _boardSession = boardSession;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _boardSession.GameEnded += OnGameEnded;
        }

        public void Leave()
        {
            _boardSession.GameEnded -= OnGameEnded;
        }

        private void OnGameEnded()
        {
            _stateMachine.ChangeState<GameCompleteState>();
        }
    }
}
