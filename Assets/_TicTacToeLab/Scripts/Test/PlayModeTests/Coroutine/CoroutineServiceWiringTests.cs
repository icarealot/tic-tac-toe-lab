#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CoroutineServiceWiringTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator The_coroutine_service_resolves_from_the_component_factory_service_in_the_real_scene()
        {
            yield return IE_LoadScene();

            ComponentFactoryService componentFactoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            CoroutineService coroutineService = componentFactoryService.Get<CoroutineService>();

            Assert.That(coroutineService, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator The_scene_starts_with_one_root_coroutine_service_resolvable_through_its_interface()
        {
            yield return IE_LoadScene();

            CoroutineService[] startupServices = Object.FindObjectsByType<CoroutineService>(FindObjectsSortMode.None);

            Assert.That(startupServices.Length, Is.EqualTo(1));
            Assert.That(startupServices[0].transform.parent, Is.Null);

            IFactoryService factoryService = Object.FindFirstObjectByType<FactoryService>();
            ICoroutineService created = factoryService.Get<ICoroutineService>();

            Assert.That(created, Is.TypeOf<CoroutineService>());
            Assert.That(created, Is.Not.SameAs(startupServices[0]));

            factoryService.Return(created);
        }
    }
}
#endif
