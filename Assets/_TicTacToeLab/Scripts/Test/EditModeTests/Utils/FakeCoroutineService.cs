using System;
using System.Collections;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeCoroutineService : ICoroutineService
    {
        public bool HasCapturedRoutine => _routine != null;
        public bool HasScheduledCallback => _callback != null;
        public float ScheduledDelaySeconds { get; private set; }
        public bool WasStopped => _stopped;

        private IEnumerator _routine;
        private Action _callback;
        private bool _stopped;

        public CoroutineHandle Run(IEnumerator routine)
        {
            _routine = routine;
            _callback = null;
            _stopped = false;
            return new CoroutineHandle(() => _stopped = true);
        }

        public CoroutineHandle RunAfter(float delaySeconds, Action callback)
        {
            _routine = null;
            _callback = callback;
            ScheduledDelaySeconds = delaySeconds;
            _stopped = false;
            return new CoroutineHandle(() => _stopped = true);
        }

        public void FireScheduledCallback()
        {
            if (_stopped || _callback == null)
            {
                return;
            }

            Action callback = _callback;
            _callback = null;
            callback();
        }
    }
}
