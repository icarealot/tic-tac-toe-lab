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
        private CoroutineService _coroutineService;

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            _coroutineService = new GameObject(nameof(CoroutineService)).AddComponent<CoroutineService>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            UnityEngine.Object.Destroy(_coroutineService.gameObject);
            yield return null;
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
        public IEnumerator Disposing_the_handle_before_the_routine_completes_stops_it()
        {
            bool ranAfterYield = false;
            CoroutineHandle handle = _coroutineService.Run(IE_SetFlagAfterYield(() => ranAfterYield = true));

            handle.Dispose();

            yield return null;
            yield return null;

            Assert.That(ranAfterYield, Is.False);
        }

        [UnityTest]
        public IEnumerator Disposing_a_handle_whose_routine_has_already_finished_does_nothing()
        {
            CoroutineHandle handle = _coroutineService.Run(IE_Immediate());

            yield return null;
            yield return null;

            Assert.DoesNotThrow(() => handle.Dispose());
        }

        [UnityTest]
        public IEnumerator Disposing_the_same_handle_twice_does_nothing()
        {
            CoroutineHandle handle = _coroutineService.Run(IE_Immediate());

            handle.Dispose();

            Assert.DoesNotThrow(() => handle.Dispose());
            yield return null;
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

        private IEnumerator IE_Immediate()
        {
            yield break;
        }
    }
}
