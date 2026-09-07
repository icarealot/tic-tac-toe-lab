using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeStateMachine : IStateMachine
    {
        public Type ChangedStateType { get; private set; }
        public int ChangeStateCount { get; private set; }

        public void ChangeState<TState>() where TState : IAppState
        {
            ChangedStateType = typeof(TState);
            ChangeStateCount++;
        }
    }
}
