using System;

namespace TicTacToeLab.Runtime
{
    public sealed class CoroutineHandle : IDisposable
    {
        private readonly Action _stop;
        private CoroutineHandleState _state;

        public CoroutineHandle(Action stop)
        {
            _stop = stop ?? throw new ArgumentNullException(nameof(stop));
        }

        public void MarkFinished()
        {
            if (_state == CoroutineHandleState.Running)
            {
                _state = CoroutineHandleState.Finished;
            }
        }

        public void Dispose()
        {
            if (_state == CoroutineHandleState.Disposed)
            {
                return;
            }

            bool wasRunning = _state == CoroutineHandleState.Running;
            _state = CoroutineHandleState.Disposed;

            if (wasRunning)
            {
                _stop();
            }
        }
    }
}
