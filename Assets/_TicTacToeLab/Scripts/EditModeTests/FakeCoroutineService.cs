using System.Collections;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    internal sealed class FakeCoroutineService : ICoroutineService
    {
        public bool HasCapturedRoutine => _routine != null;
        public bool WasStopped => _stopped;

        private IEnumerator _routine;
        private bool _stopped;

        public CoroutineHandle Run(IEnumerator routine)
        {
            _routine = routine;
            _stopped = false;
            return new CoroutineHandle(() => _stopped = true);
        }

        public void PumpToCompletion()
        {
            while (!_stopped && _routine.MoveNext())
            {
            }
        }
    }
}
