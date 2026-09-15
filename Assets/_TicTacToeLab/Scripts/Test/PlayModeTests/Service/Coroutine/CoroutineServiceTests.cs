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
    public sealed class CoroutineServiceTests
    {
        private CoroutineService _sut;

        [OneTimeSetUp]
        public void CreateIsolatedService()
        {
            _sut = new GameObject("CoroutineServiceTests").AddComponent<CoroutineService>();
        }

        [OneTimeTearDown]
        public void DestroyIsolatedService()
        {
            UnityEngine.Object.Destroy(_sut.gameObject);
        }

        [UnityTest]
        public IEnumerator A_routine_handed_to_the_service_runs_and_continues_across_frames()
        {
            // Arrange
            List<int> steps = new();

            // Act
            _ = _sut.Run(IE_RecordSteps(steps));

            // Assert
            Assert.That(steps, Is.EqualTo(new[] { 1 }));
            yield return null;
            Assert.That(steps, Is.EqualTo(new[] { 1, 2 }));
        }

        [UnityTest]
        public IEnumerator A_callback_scheduled_after_a_short_delay_is_deferred_and_then_runs()
        {
            // Arrange
            bool callbackRan = false;

            // Act
            _ = _sut.RunAfter(0.01f, () => callbackRan = true);

            // Assert
            Assert.That(callbackRan, Is.False);

            // Poll instead of waiting a fixed duration: eventual execution is observed rather than assumed from frame pacing, and the one-second deadline only bounds failure.
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
            // Arrange
            bool ranAfterYield = false;

            // Act
            CoroutineHandle handle = _sut.Run(IE_SetFlagAfterYield(() => ranAfterYield = true));
            handle.Dispose();

            // Assert
            yield return null;

            Assert.That(ranAfterYield, Is.False);
        }

        private static IEnumerator IE_RecordSteps(List<int> steps)
        {
            steps.Add(1);
            yield return null;
            steps.Add(2);
        }

        private static IEnumerator IE_SetFlagAfterYield(Action setFlag)
        {
            yield return null;
            setFlag();
        }
    }
}
#endif
