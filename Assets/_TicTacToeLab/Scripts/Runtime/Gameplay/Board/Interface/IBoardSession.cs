using System;

namespace TicTacToeLab.Runtime
{
    public interface IBoardSession
    {
        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public Mark Turn { get; }
        public Outcome Outcome { get; }

        public void Reset();
    }
}
