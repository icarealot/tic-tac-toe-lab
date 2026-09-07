#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CoroutineServiceWiringTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator The_coroutine_service_resolves_from_the_component_factory_service_in_the_real_scene()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            ComponentFactoryService factoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            CoroutineService coroutineService = factoryService.Get<CoroutineService>();

            Assert.That(coroutineService, Is.Not.Null);
        }
    }
}
#endif
