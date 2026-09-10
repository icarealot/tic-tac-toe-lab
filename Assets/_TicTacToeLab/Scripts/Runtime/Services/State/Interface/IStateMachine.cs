namespace TicTacToeLab.Runtime
{
    public interface IStateMachine
    {
        public void ChangeState<TState>() where TState : IAppState;
    }
}
