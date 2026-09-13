using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class RecordingStateMachine : IStateMachine
    {
        private readonly List<string> _log;

        public RecordingStateMachine(List<string> log)
        {
            _log = log;
        }

        public void ChangeState<TState>() where TState : IAppState
        {
            _log.Add($"StateMachine.ChangeState<{typeof(TState).Name}>");
        }
    }
}
