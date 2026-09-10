#if UNITY_EDITOR
using System;
using System.Collections;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Records how many routines were run and callbacks scheduled, without running either. Shared
    /// by the PlayMode tests so the recorder is defined once rather than per test class.
    /// </summary>
    public sealed class RecordingCoroutineService : ICoroutineService
    {
        public int RunCount { get; private set; }
        public int RunAfterCount { get; private set; }

        public CoroutineHandle Run(IEnumerator routine)
        {
            RunCount++;
            return new CoroutineHandle(() => { });
        }

        public CoroutineHandle RunAfter(float delaySeconds, Action callback)
        {
            RunAfterCount++;
            return new CoroutineHandle(() => { });
        }
    }
}
#endif
