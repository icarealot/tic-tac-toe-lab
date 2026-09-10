namespace TicTacToeLab.Runtime
{
    public interface IAppState
    {
        public void Enter();

        public void Leave();

        public void Back() { }
    }
}
