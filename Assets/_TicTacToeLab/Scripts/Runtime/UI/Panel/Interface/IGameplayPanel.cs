using System;

namespace TicTacToeLab.Runtime
{
    public interface IGameplayPanel : IPanel
    {
        public void Setup(IBoardSession boardSession, Action onBack);
    }
}
