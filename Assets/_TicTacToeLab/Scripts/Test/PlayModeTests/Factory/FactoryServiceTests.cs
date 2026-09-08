#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class FactoryServiceTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_uniquely_registered_interface_produces_a_new_implementation_from_its_prefab()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();
            CoroutineService productionInstance = UnityEngine.Object.FindFirstObjectByType<CoroutineService>();
            ICoroutineService created = factoryService.Get<ICoroutineService>();

            Assert.That(created, Is.Not.Null);
            Assert.That(created, Is.TypeOf<CoroutineService>());
            Assert.That((Component)created, Is.Not.SameAs(productionInstance));
            Assert.That(((Component)created).transform.parent, Is.Null);

            factoryService.Return(created);
        }

        [UnityTest]
        public IEnumerator A_supplied_parent_is_applied_during_instantiation()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();
            Transform parent = new GameObject("Parent").transform;

            ICoroutineService created = factoryService.Get<ICoroutineService>(parent);

            Assert.That(((Component)created).transform.parent, Is.EqualTo(parent));

            factoryService.Return(created);
            UnityEngine.Object.Destroy(parent.gameObject);
        }

        [UnityTest]
        public IEnumerator Returning_an_interface_backed_object_destroys_its_gameobject()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();
            IGameplayPanel panel = factoryService.Get<IGameplayPanel>();
            Component panelComponent = (Component)panel;
            GameObject panelGameObject = panelComponent.gameObject;

            factoryService.Return(panel);
            yield return null;

            Assert.That(panelGameObject == null, Is.True);
            Assert.That(panelComponent == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Returning_an_object_that_is_not_a_unity_component_throws()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();

            Assert.That(() => factoryService.Return(new PlainImplementation()), Throws.TypeOf<InvalidOperationException>());
        }

        [UnityTest]
        public IEnumerator Requesting_a_concrete_type_is_rejected_even_when_it_is_registered()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();

            Assert.That(() => factoryService.Get<CoroutineService>(), Throws.TypeOf<InvalidOperationException>());
        }

        [UnityTest]
        public IEnumerator Requesting_an_interface_without_a_registered_implementation_throws()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();

            Assert.That(() => factoryService.Get<IUnregisteredRole>(), Throws.TypeOf<InvalidOperationException>());
        }

        [UnityTest]
        public IEnumerator An_interface_matched_by_multiple_registered_prefabs_throws_regardless_of_registry_order()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();

            // Both implementations of IWindow are registered and individually resolvable,
            // so the broad role is genuinely ambiguous rather than merely unregistered.
            IGameplayPanel panel = factoryService.Get<IGameplayPanel>();
            IConfirmQuitPopup popup = factoryService.Get<IConfirmQuitPopup>();

            Assert.That(panel, Is.Not.Null);
            Assert.That(popup, Is.Not.Null);

            Assert.That(() => factoryService.Get<IWindow>(), Throws.TypeOf<InvalidOperationException>());

            factoryService.Return(panel);
            factoryService.Return(popup);
        }

        private interface IUnregisteredRole
        {
        }

        private sealed class PlainImplementation : IUnregisteredRole
        {
        }
    }
}
#endif
