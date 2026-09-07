using System;

namespace TicTacToeLab.Runtime
{
    public interface IBoardSession
    {
        public Mark Turn { get; }
        public event Action GameEnded;
        public event Action<Mark> TurnChanged;
        public void Reset();
    }
}
