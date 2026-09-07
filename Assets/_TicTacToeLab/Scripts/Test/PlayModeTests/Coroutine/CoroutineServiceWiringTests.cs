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
    }
}
#endif
