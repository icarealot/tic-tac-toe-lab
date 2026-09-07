using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeBoardSession : IBoardSession
    {
        public Mark Turn { get; private set; } = Mark.X;
        public bool WasReset { get; private set; }
        public int ResetCount { get; private set; }

        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public void Reset()
        {
            WasReset = true;
            ResetCount++;
            Turn = Mark.X;
        }

        public void RaiseGameEnded()
        {
            GameEnded?.Invoke();
        }

        public void RaiseTurnChanged(Mark turn)
        {
            Turn = turn;
            TurnChanged?.Invoke(turn);
        }
    }
}
