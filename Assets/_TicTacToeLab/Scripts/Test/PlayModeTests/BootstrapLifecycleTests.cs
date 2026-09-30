#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BootstrapLifecycleTests
    {
        private const int ADAPTER_ROOT_COUNT = 4;
        private const string MISSING_ADAPTER_FIELD = "_delaySchedulerPrefab";

        private GeneratedBootstrapFixture _fixture;

        [SetUp]
        public void CreateGeneratedBootstrapFixture()
        {
            _fixture = new GeneratedBootstrapFixture();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedBootstrapFixture()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [UnityTest]
        public IEnumerator Destroying_the_bootstrap_destroys_every_adapter_root_and_releases_the_input_actions()
        {
            // Arrange
            InputActionAsset[] assetsBeforeStartup = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            Bootstrap sut = _fixture.CreateConfiguredBootstrap();
            InputActionAsset[] inputServiceActions = InputActionAssetsCreatedSince(assetsBeforeStartup);
            Assert.That(
                inputServiceActions,
                Has.Length.EqualTo(1),
                "Starting Bootstrap should create exactly the input service's actions.");
            GameObject[] adapterRoots = _fixture.OwnedAdapterRoots().Select(adapter => adapter.gameObject).ToArray();
            Assert.That(
                adapterRoots,
                Has.Length.EqualTo(ADAPTER_ROOT_COUNT),
                "A complete generated configuration should create one root for each long-lived adapter.");

            // Act
            UnityEngine.Object.Destroy(sut.gameObject);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => adapterRoots.All(adapterRoot => adapterRoot == null),
                "Destroying Bootstrap should destroy every adapter root it created.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => inputServiceActions.All(actions => actions == null),
                "Destroying Bootstrap should release the input service's Input System actions.");
        }

        [Test]
        public void A_complete_generated_configuration_starts_at_Home()
        {
            // Act
            _ = _fixture.CreateConfiguredBootstrap();

            // Assert
            LifecycleProbe home = _fixture.AppUI.Screens.RequireLatestFor<HomeScreen>();
            Assert.That(home.IsShown, Is.True, "Starting the application should present Home.");
            Assert.That(
                _fixture.AppUI.Screens.LatestFor<GameplayScreen>(),
                Is.Null,
                "Starting the application should not present gameplay before the player starts a game.");
        }

        [Test]
        public void An_incomplete_configuration_fails_startup_without_creating_any_adapter_root_or_input_actions()
        {
            // Arrange
            Bootstrap sut = _fixture.CreateBootstrapHost();
            _fixture.AssignAdapterTemplates(sut);
            TestSerializedReference.ClearReference(sut, MISSING_ADAPTER_FIELD);
            InputActionAsset[] assetsBeforeStartup = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            Assert.That(
                sut.gameObject.activeInHierarchy,
                Is.False,
                "The failure fixture should begin inactive so its configuration can change before Awake runs.");

            // Act and Assert
            Assert.That(
                () => sut.Awake(),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("DelayScheduler adapter prefab is not assigned"),
                "Missing adapter configuration should identify the delay scheduler role.");

            // Assert
            Assert.That(
                _fixture.OwnedAdapterRoots(),
                Is.Empty,
                "An incomplete configuration should fail before any adapter root is created.");
            Assert.That(
                InputActionAssetsCreatedSince(assetsBeforeStartup),
                Is.Empty,
                "An incomplete configuration should fail before the input service's actions are created.");
        }

        private static InputActionAsset[] InputActionAssetsCreatedSince(InputActionAsset[] assetsBeforeStartup)
        {
            return Resources.FindObjectsOfTypeAll<InputActionAsset>()
                .Where(asset => !assetsBeforeStartup.Contains(asset))
                .ToArray();
        }
    }
}
#endif
