#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ComponentFactoryServiceTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_registered_component_is_resolved_from_the_prefab_registry()
        {
            yield return IE_LoadScene();

            IComponentFactoryService componentFactoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            CoroutineService coroutineService = componentFactoryService.Get<CoroutineService>();

            Assert.That(coroutineService, Is.Not.Null);

            componentFactoryService.Return(coroutineService);
        }

        [UnityTest]
        public IEnumerator Returning_a_component_destroys_the_instance()
        {
            yield return IE_LoadScene();

            IComponentFactoryService componentFactoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            MarkView markView = componentFactoryService.Get<MarkView>();

            componentFactoryService.Return(markView);
            yield return null;

            Assert.That(markView == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Resolving_an_unregistered_component_throws()
        {
            yield return IE_LoadScene();

            IComponentFactoryService componentFactoryService = Object.FindFirstObjectByType<ComponentFactoryService>();

            Assert.That(() => componentFactoryService.Get<UnregisteredComponent>(), Throws.TypeOf<System.InvalidOperationException>());
        }

        private sealed class UnregisteredComponent : MonoBehaviour
        {
        }
    }
}
#endif
