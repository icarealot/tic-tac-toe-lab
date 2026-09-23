#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class DelaySchedulerTests
    {
        private const float SHORT_DELAY_SECONDS = 0.05f;

        private DelayScheduler _sut;

        [SetUp]
        public void CreateIsolatedScheduler()
        {
            _sut = new GameObject("DelaySchedulerTests").AddComponent<DelayScheduler>();
        }

        [UnityTearDown]
        public IEnumerator DestroyIsolatedScheduler()
        {
            if (_sut == null)
            {
                yield break;
            }

            UnityEngine.Object.Destroy(_sut.gameObject);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _sut == null,
                "The scheduler fixture should be destroyed after the test.");
        }

        [UnityTest]
        public IEnumerator A_scheduled_callback_is_deferred_and_runs_at_most_once()
        {
            // Arrange
            int callbackCount = 0;

            // Act
            _ = _sut.Schedule(SHORT_DELAY_SECONDS, () => callbackCount++);

            // Assert
            Assert.That(callbackCount, Is.EqualTo(0), "The callback should not run before the delay elapses.");

            yield return IE_WaitForSentinelCallback();

            Assert.That(callbackCount, Is.EqualTo(1), "The callback should run at most once.");
        }

        [UnityTest]
        public IEnumerator Disposing_pending_work_prevents_its_callback()
        {
            // Arrange
            int callbackCount = 0;
            IDisposable scheduledCallback = _sut.Schedule(SHORT_DELAY_SECONDS, () => callbackCount++);

            // Act
            scheduledCallback.Dispose();

            // Assert
            yield return IE_WaitForSentinelCallback();

            Assert.That(callbackCount, Is.EqualTo(0), "Disposing pending work should prevent its callback.");
        }

        [UnityTest]
        public IEnumerator Disposing_after_completion_and_repeated_disposal_are_harmless()
        {
            // Arrange
            int callbackCount = 0;
            IDisposable scheduledCallback = _sut.Schedule(SHORT_DELAY_SECONDS, () => callbackCount++);

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => callbackCount == 1,
                "The callback should run once after the delay elapses.");

            // Act
            TestDelegate disposeRepeatedly = () =>
            {
                scheduledCallback.Dispose();
                scheduledCallback.Dispose();
            };

            // Assert
            Assert.That(disposeRepeatedly, Throws.Nothing, "Disposing after completion and repeatedly should be harmless.");
            Assert.That(callbackCount, Is.EqualTo(1), "Disposal should not run the callback again or undo its completion.");
        }

        // A later callback scheduled after the subject gives a bounded, observable point in
        // time: once this sentinel is delivered, the subject's delay window has elapsed.
        private IEnumerator IE_WaitForSentinelCallback()
        {
            bool sentinelRan = false;
            _ = _sut.Schedule(SHORT_DELAY_SECONDS, () => sentinelRan = true);

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => sentinelRan,
                "The scheduler should deliver a later callback after the same delay.");
        }
    }
}
#endif
