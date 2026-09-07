using System;
using System.Collections;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class CoroutineService : MonoBehaviour, ICoroutineService
    {
        public CoroutineHandle Run(IEnumerator routine)
        {
            Coroutine coroutine = default;
            CoroutineHandle handle = new(() =>
            {
                if (this != null)
                {
                    StopCoroutine(coroutine);
                }
            });

            coroutine = StartCoroutine(IE_TrackCompletion(routine, handle));

            return handle;
        }

        public CoroutineHandle RunAfter(float delaySeconds, Action callback)
        {
            return Run(IE_RunAfter(delaySeconds, callback));
        }

        private IEnumerator IE_RunAfter(float delaySeconds, Action callback)
        {
            yield return new WaitForSeconds(delaySeconds);
            callback();
        }

        private IEnumerator IE_TrackCompletion(IEnumerator routine, CoroutineHandle handle)
        {
            yield return routine;
            handle.MarkFinished();
        }
    }
}
