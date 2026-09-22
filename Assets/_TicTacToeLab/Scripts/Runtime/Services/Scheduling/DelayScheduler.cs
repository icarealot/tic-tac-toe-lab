using System;
using System.Collections;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class DelayScheduler : MonoBehaviour, IDelayScheduler
    {
        public IDisposable Schedule(float delaySeconds, Action callback)
        {
            ScheduledCallback scheduledCallback = new(callback);
            _ = StartCoroutine(IE_RunAfter(delaySeconds, scheduledCallback));
            return scheduledCallback;
        }

        private IEnumerator IE_RunAfter(float delaySeconds, ScheduledCallback scheduledCallback)
        {
            yield return new WaitForSeconds(delaySeconds);
            scheduledCallback.Invoke();
        }

        private sealed class ScheduledCallback : IDisposable
        {
            private readonly Action _callback;
            private bool _isSettled;

            public ScheduledCallback(Action callback)
            {
                _callback = callback;
            }

            public void Invoke()
            {
                if (_isSettled)
                {
                    return;
                }

                _isSettled = true;
                _callback();
            }

            public void Dispose()
            {
                _isSettled = true;
            }
        }
    }
}
