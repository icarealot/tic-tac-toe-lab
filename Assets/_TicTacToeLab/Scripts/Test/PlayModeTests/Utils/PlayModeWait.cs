#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public static class PlayModeWait
    {
        private const float TIMEOUT_SECONDS = 3f;

        public static IEnumerator IE_WaitUntilOrFail(Func<bool> condition, string failureMessage)
        {
            float deadline = Time.realtimeSinceStartup + TIMEOUT_SECONDS;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(condition(), Is.True, failureMessage);
        }
    }
}
#endif
