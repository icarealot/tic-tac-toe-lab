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
    public sealed class FactoryServiceTests
    {
        private interface IUnregisteredRole
        {
        }

        private sealed class PlainImplementation : IUnregisteredRole
        {
        }

        private const string FACTORY_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/Services/Factory.prefab";

        private FactoryService _sut;

        [SetUp]
        public void InstantiateProductionFactory()
        {
            GameObject factoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FACTORY_PREFAB_PATH);
            Assert.That(factoryPrefab, Is.Not.Null, "The production factory prefab must exist at its shipped path.");

            _sut = UnityEngine.Object.Instantiate(factoryPrefab).GetComponent<FactoryService>();
            Assert.That(_sut, Is.Not.Null, "The factory prefab must carry the FactoryService.");
        }

        [TearDown]
        public void DestroyFactoryInstance()
        {
            if (_sut != null)
            {
                UnityEngine.Object.Destroy(_sut.gameObject);
            }
        }

        // --- Requesting ---

        [Test]
        public void A_uniquely_registered_interface_produces_a_new_implementation_from_its_prefab()
        {
            // Act
            ICoroutineService first = _sut.Get<ICoroutineService>();
            ICoroutineService second = _sut.Get<ICoroutineService>();
            Component firstComponent = (Component)first;
            Component secondComponent = (Component)second;

            // Assert
            Assert.That(secondComponent != firstComponent, Is.True);
            Assert.That(firstComponent.transform.parent, Is.Null);

            _sut.Return(first);
            _sut.Return(second);
        }

        [Test]
        public void A_supplied_parent_is_applied_during_instantiation()
        {
            Transform parent = new GameObject("Parent").transform;

            ICoroutineService created = _sut.Get<ICoroutineService>(parent);

            Assert.That(((Component)created).transform.parent, Is.EqualTo(parent));

            UnityEngine.Object.Destroy(parent.gameObject);
        }

        [Test]
        public void Requesting_a_concrete_type_is_rejected_even_when_it_is_registered()
        {
            AssertFactoryRejects(() => _sut.Get<CoroutineService>());
        }

        [Test]
        public void Requesting_an_interface_without_a_registered_implementation_throws()
        {
            AssertFactoryRejects(() => _sut.Get<IUnregisteredRole>());
        }

        [Test]
        public void An_interface_matched_by_multiple_registered_prefabs_throws_regardless_of_registry_order()
        {
            // Arrange
            IGameplayPanel panel = _sut.Get<IGameplayPanel>();
            IConfirmQuitPopup popup = _sut.Get<IConfirmQuitPopup>();

            // Both implementations of IWindow are registered and individually resolvable, so the broad role is genuinely ambiguous rather than merely unregistered.
            Assert.That(panel, Is.Not.Null);
            Assert.That(popup, Is.Not.Null);

            // Act
            TestDelegate requestAmbiguousRole = () => _sut.Get<IWindow>();

            // Assert
            AssertFactoryRejects(requestAmbiguousRole);

            _sut.Return(panel);
            _sut.Return(popup);
        }

        // --- Returning ---

        [UnityTest]
        public IEnumerator Returning_an_interface_backed_object_destroys_its_gameobject()
        {
            // Arrange
            IGameplayPanel panel = _sut.Get<IGameplayPanel>();
            GameObject panelGameObject = ((Component)panel).gameObject;

            // Act
            _sut.Return(panel);
            yield return null;

            // Assert
            Assert.That(panelGameObject == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Returning_an_already_destroyed_interface_backed_object_is_a_no_op()
        {
            IGameplayPanel panel = _sut.Get<IGameplayPanel>();

            UnityEngine.Object.Destroy((Component)panel);
            yield return null;

            Assert.That(() => _sut.Return(panel), Throws.Nothing);
        }

        [Test]
        public void Returning_an_object_that_is_not_a_unity_component_throws()
        {
            IUnregisteredRole plain = new PlainImplementation();

            AssertFactoryRejects(() => _sut.Return(plain));
        }

        [Test]
        public void Returning_a_concrete_type_is_rejected_even_when_it_is_registered()
        {
            // Arrange
            CoroutineService concreteInstance = (CoroutineService)_sut.Get<ICoroutineService>();

            // Act
            AssertFactoryRejects(() => _sut.Return(concreteInstance));

            // Assert
            Assert.That(concreteInstance == null, Is.False);

            UnityEngine.Object.Destroy(concreteInstance.gameObject);
        }

        private void AssertFactoryRejects(TestDelegate operation)
        {
            Assert.That(operation, Throws.TypeOf<InvalidOperationException>());
        }
    }
}
#endif
