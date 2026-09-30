namespace TicTacToeLab.Runtime
{
    public interface IGameplayTurnController
    {
        public void Enter();
        public void Pause();
        public void Resume();
        public void Exit();
    }
}
