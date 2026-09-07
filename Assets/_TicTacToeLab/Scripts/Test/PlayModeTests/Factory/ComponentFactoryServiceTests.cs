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
    public sealed class ComponentFactoryServiceTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_registered_component_is_resolved_from_the_prefab_registry()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            IComponentFactoryService factoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            CoroutineService coroutineService = factoryService.Get<CoroutineService>();

            Assert.That(coroutineService, Is.Not.Null);

            factoryService.Return(coroutineService);
        }

        [UnityTest]
        public IEnumerator Returning_a_component_destroys_the_instance()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            IComponentFactoryService factoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            MarkView markView = factoryService.Get<MarkView>();

            factoryService.Return(markView);
            yield return null;

            Assert.That(markView == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Resolving_an_unregistered_component_throws()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            IComponentFactoryService factoryService = Object.FindFirstObjectByType<ComponentFactoryService>();

            Assert.That(() => factoryService.Get<UnregisteredComponent>(), Throws.TypeOf<System.InvalidOperationException>());
        }

        private sealed class UnregisteredComponent : MonoBehaviour
        {
        }
    }
}
#endif
