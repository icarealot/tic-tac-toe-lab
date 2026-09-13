#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// CoroutineService frame behavior on one isolated component: a routine executes before and
    /// continues after a yielded frame, RunAfter defers a callback and eventually runs it, and
    /// disposing the handle before continuation stops the routine for good. No production scene
    /// or prefab participates in this timing mechanism. CoroutineHandle state transitions
    /// (stop-once, repeated disposal, completed-before-disposal) are plain EditMode tests.
    /// </summary>
    public sealed class CoroutineServiceTests
    {
        private CoroutineService _coroutineService;

        [OneTimeSetUp]
        public void CreateIsolatedService()
        {
            _coroutineService = new GameObject("CoroutineServiceTests")
                .AddComponent<CoroutineService>();
        }

        [OneTimeTearDown]
        public void DestroyIsolatedService()
        {
            UnityEngine.Object.Destroy(_coroutineService.gameObject);
        }

        [UnityTest]
        public IEnumerator A_routine_handed_to_the_service_runs_and_continues_across_frames()
        {
            List<int> steps = new();

            _ = _coroutineService.Run(IE_RecordSteps(steps));
            Assert.That(steps, Is.EqualTo(new[] { 1 }));

            yield return null;
            Assert.That(steps, Is.EqualTo(new[] { 1, 2 }));
        }

        [UnityTest]
        public IEnumerator A_callback_scheduled_after_a_short_delay_is_deferred_and_then_runs()
        {
            bool callbackRan = false;

            _ = _coroutineService.RunAfter(0.01f, () => callbackRan = true);
            Assert.That(callbackRan, Is.False);

            // Poll instead of waiting a fixed duration: eventual execution is observed rather
            // than assumed from frame pacing, and the one-second deadline only bounds failure.
            float deadline = Time.realtimeSinceStartup + 1f;
            while (!callbackRan && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(callbackRan, Is.True);
        }

        [UnityTest]
        public IEnumerator Disposing_the_handle_before_the_routine_continues_stops_it()
        {
            bool ranAfterYield = false;
            CoroutineHandle handle = _coroutineService.Run(IE_SetFlagAfterYield(() => ranAfterYield = true));

            handle.Dispose();

            yield return null;
            yield return null;

            Assert.That(ranAfterYield, Is.False);
        }

        private IEnumerator IE_RecordSteps(List<int> steps)
        {
            steps.Add(1);
            yield return null;
            steps.Add(2);
        }

        private IEnumerator IE_SetFlagAfterYield(Action setFlag)
        {
            yield return null;
            setFlag();
        }
    }
}
#endif
