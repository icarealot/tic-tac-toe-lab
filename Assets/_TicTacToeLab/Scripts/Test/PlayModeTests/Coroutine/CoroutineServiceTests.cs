#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CoroutineServiceTests : SceneWiringTests
    {
        private CoroutineService _coroutineService;

        [UnityTest]
        public IEnumerator A_routine_handed_to_the_service_runs_and_continues_across_frames()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            _coroutineService = UnityEngine.Object.FindFirstObjectByType<CoroutineService>();
            List<int> steps = new();

            _ = _coroutineService.Run(IE_RecordSteps(steps));
            Assert.That(steps, Is.EqualTo(new[] { 1 }));

            yield return null;
            Assert.That(steps, Is.EqualTo(new[] { 1, 2 }));
        }

        [UnityTest]
        public IEnumerator A_callback_scheduled_after_a_delay_runs_across_real_frames()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            _coroutineService = UnityEngine.Object.FindFirstObjectByType<CoroutineService>();
            bool callbackRan = false;

            _ = _coroutineService.RunAfter(0.1f, () => callbackRan = true);
            Assert.That(callbackRan, Is.False);

            yield return new UnityEngine.WaitForSeconds(0.2f);

            Assert.That(callbackRan, Is.True);
        }

        [UnityTest]
        public IEnumerator Disposing_the_handle_before_the_routine_completes_stops_it()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            _coroutineService = UnityEngine.Object.FindFirstObjectByType<CoroutineService>();
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
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            _coroutineService = UnityEngine.Object.FindFirstObjectByType<CoroutineService>();
            CoroutineHandle handle = _coroutineService.Run(IE_Immediate());

            yield return null;
            yield return null;

            Assert.DoesNotThrow(() => handle.Dispose());
        }

        [UnityTest]
        public IEnumerator Disposing_the_same_handle_twice_does_nothing()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            _coroutineService = UnityEngine.Object.FindFirstObjectByType<CoroutineService>();
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
#endif
