namespace TicTacToeLab.Runtime
{
    public sealed class GameCompleteState : IAppState
    {
        private readonly BoardSession _boardSession;
        private readonly AppStateMachine _stateMachine;

        public GameCompleteState(BoardSession boardSession, AppStateMachine stateMachine)
        {
            _boardSession = boardSession;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _stateMachine.ChangeState<GameplayState>();
        }

        public void Leave()
        {
            _boardSession.Reset();
        }
    }
}
