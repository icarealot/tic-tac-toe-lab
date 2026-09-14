using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeBoardSession : IBoardSession
    {
        public Mark Turn { get; private set; } = Mark.X;

        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public void Reset()
        {
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
