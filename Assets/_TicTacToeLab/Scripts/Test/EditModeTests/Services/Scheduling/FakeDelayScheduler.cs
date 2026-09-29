using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeDelayScheduler : IDelayScheduler
    {
        public float RequestedDelaySeconds { get; private set; }
        public int ScheduleCount { get; private set; }
        public bool HasPendingWork => _pendingCallback != null;
        public bool WasCancelled { get; private set; }

        private Action _pendingCallback;
        private Action _lastDeliveredCallback;

        public IDisposable Schedule(float delaySeconds, Action callback)
        {
            _pendingCallback = callback;
            RequestedDelaySeconds = delaySeconds;
            ScheduleCount++;
            WasCancelled = false;
            return new Cancellation(this);
        }

        public Action CapturePendingCallback()
        {
            return _pendingCallback;
        }

        public void FirePending()
        {
            if (_pendingCallback == null)
            {
                return;
            }

            Action callback = _pendingCallback;
            _pendingCallback = null;
            _lastDeliveredCallback = callback;
            callback();
        }

        public void ReplayLastDelivered()
        {
            _lastDeliveredCallback?.Invoke();
        }

        private void CancelPending()
        {
            if (_pendingCallback == null)
            {
                return;
            }

            _pendingCallback = null;
            WasCancelled = true;
        }

        private sealed class Cancellation : IDisposable
        {
            private FakeDelayScheduler _scheduler;

            public Cancellation(FakeDelayScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public void Dispose()
            {
                _scheduler?.CancelPending();
                _scheduler = null;
            }
        }
    }
}
