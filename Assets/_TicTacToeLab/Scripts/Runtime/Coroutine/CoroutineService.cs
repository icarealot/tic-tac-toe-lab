using System.Collections;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class CoroutineService : MonoBehaviour, ICoroutineService
    {
        public CoroutineHandle Run(IEnumerator routine)
        {
            Coroutine coroutine = default;
            CoroutineHandle handle = new(() => StopCoroutine(coroutine));

            coroutine = StartCoroutine(IE_TrackCompletion(routine, handle));

            return handle;
        }

        private IEnumerator IE_TrackCompletion(IEnumerator routine, CoroutineHandle handle)
        {
            yield return routine;
            handle.MarkFinished();
        }
    }
}
