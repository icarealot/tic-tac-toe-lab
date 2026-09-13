#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// FactoryService contracts exercised against the production factory prefab's serialized role
    /// registry, instantiated directly instead of reached through application-scene bootstrap: the
    /// registry is the configuration under test. Assertions stay on role resolution and return
    /// lifecycle and never reach unrelated registry entries or visual prefab content. Frameless
    /// contracts run as plain tests; only return destruction needs a frame, so only those two run
    /// as coroutines.
    /// </summary>
    public sealed class FactoryServiceTests
    {
        private interface IUnregisteredRole { }

        private sealed class PlainImplementation : IUnregisteredRole { }

        private const string FACTORY_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/Services/Factory.prefab";

        private FactoryService _factoryService;

        [SetUp]
        public void InstantiateProductionFactory()
        {
            GameObject factoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FACTORY_PREFAB_PATH);
            Assert.That(factoryPrefab, Is.Not.Null, "The production factory prefab must exist at its shipped path.");

            _factoryService = UnityEngine.Object.Instantiate(factoryPrefab).GetComponent<FactoryService>();
            Assert.That(_factoryService, Is.Not.Null, "The factory prefab must carry the FactoryService.");
        }

        [TearDown]
        public void DestroyFactoryInstance()
        {
            if (_factoryService != null)
            {
                UnityEngine.Object.Destroy(_factoryService.gameObject);
            }
        }

        [Test]
        public void A_uniquely_registered_interface_produces_a_new_implementation_from_its_prefab()
        {
            ICoroutineService first = _factoryService.Get<ICoroutineService>();
            ICoroutineService second = _factoryService.Get<ICoroutineService>();
            Component firstComponent = (Component)first;
            Component secondComponent = (Component)second;

            // A repeat request creates another instance rather than reusing one.
            Assert.That(secondComponent != firstComponent, Is.True);
            Assert.That(firstComponent.transform.parent, Is.Null);

            _factoryService.Return(first);
            _factoryService.Return(second);
        }

        [Test]
        public void A_supplied_parent_is_applied_during_instantiation()
        {
            Transform parent = new GameObject("Parent").transform;

            ICoroutineService created = _factoryService.Get<ICoroutineService>(parent);

            Assert.That(((Component)created).transform.parent, Is.EqualTo(parent));

            UnityEngine.Object.Destroy(parent.gameObject);
        }

        [UnityTest]
        public IEnumerator Returning_an_interface_backed_object_destroys_its_gameobject()
        {
            IGameplayPanel panel = _factoryService.Get<IGameplayPanel>();
            GameObject panelGameObject = ((Component)panel).gameObject;

            _factoryService.Return(panel);
            yield return null;

            Assert.That(panelGameObject == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Returning_an_already_destroyed_interface_backed_object_is_a_no_op()
        {
            IGameplayPanel panel = _factoryService.Get<IGameplayPanel>();

            UnityEngine.Object.Destroy((Component)panel);
            yield return null;

            Assert.That(() => _factoryService.Return(panel), Throws.Nothing);
        }

        [Test]
        public void Returning_an_object_that_is_not_a_unity_component_throws()
        {
            IUnregisteredRole plain = new PlainImplementation();

            AssertFactoryRejects(() => _factoryService.Return(plain));
        }

        [Test]
        public void Requesting_a_concrete_type_is_rejected_even_when_it_is_registered()
        {
            AssertFactoryRejects(() => _factoryService.Get<CoroutineService>());
        }

        [Test]
        public void Returning_a_concrete_type_is_rejected_even_when_it_is_registered()
        {
            CoroutineService concreteInstance = (CoroutineService)_factoryService.Get<ICoroutineService>();

            AssertFactoryRejects(() => _factoryService.Return(concreteInstance));
            Assert.That(concreteInstance == null, Is.False);

            UnityEngine.Object.Destroy(concreteInstance.gameObject);
        }

        [Test]
        public void Requesting_an_interface_without_a_registered_implementation_throws()
        {
            AssertFactoryRejects(() => _factoryService.Get<IUnregisteredRole>());
        }

        [Test]
        public void An_interface_matched_by_multiple_registered_prefabs_throws_regardless_of_registry_order()
        {
            // Both implementations of IWindow are registered and individually resolvable,
            // so the broad role is genuinely ambiguous rather than merely unregistered.
            IGameplayPanel panel = _factoryService.Get<IGameplayPanel>();
            IConfirmQuitPopup popup = _factoryService.Get<IConfirmQuitPopup>();

            Assert.That(panel, Is.Not.Null);
            Assert.That(popup, Is.Not.Null);

            AssertFactoryRejects(() => _factoryService.Get<IWindow>());

            _factoryService.Return(panel);
            _factoryService.Return(popup);
        }

        private void AssertFactoryRejects(TestDelegate operation)
        {
            Assert.That(operation, Throws.TypeOf<InvalidOperationException>());
        }
    }
}
#endif
