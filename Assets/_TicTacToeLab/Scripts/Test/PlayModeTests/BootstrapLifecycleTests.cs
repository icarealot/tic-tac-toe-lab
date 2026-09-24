#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BootstrapLifecycleTests
    {
        private const string PRODUCTION_BOOTSTRAP_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/Bootstrap.prefab";
        private const string MISSING_ADAPTER_FIELD = "_delaySchedulerPrefab";
        private const string MISSING_ADAPTER_DIAGNOSTIC = "Bootstrap cannot start because the DelayScheduler adapter prefab is not assigned.";

        private Scene _fixtureScene;
        private Scene _previousActiveScene;

        [UnitySetUp]
        public IEnumerator Create_fixture_scene()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
            _fixtureScene = SceneManager.CreateScene("Bootstrap lifecycle fixture scene");
            _ = SceneManager.SetActiveScene(_fixtureScene);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator Destroy_the_fixture_scene()
        {
            if (_fixtureScene.IsValid())
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(_fixtureScene);

                yield return PlayModeWait.IE_WaitUntilOrFail(
                    () => unload.isDone && !AdapterComponents().Any(),
                    "The fixture scene and every adapter root in it should be destroyed after the test.");
            }

            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                _ = SceneManager.SetActiveScene(_previousActiveScene);
            }
        }

        // --- Failure atomicity ---

        [Test]
        public void An_incomplete_configuration_fails_startup_without_creating_any_adapter_root()
        {
            // Arrange
            (GameObject holder, Bootstrap sut) = CreateInactiveBootstrapFixture();
            TestSerializedReference.ClearReference(sut, MISSING_ADAPTER_FIELD);
            Assert.That(
                sut.gameObject.activeInHierarchy,
                Is.False,
                "The failure fixture should begin inactive so its configuration can change before Awake runs.");
            int adapterCountBefore = AdapterComponents().Count();

            // Act
            LogAssert.Expect(LogType.Error, new Regex($"^{Regex.Escape(MISSING_ADAPTER_DIAGNOSTIC)}$"));
            holder.SetActive(true);

            // Assert
            Assert.That(
                AdapterComponents().Count(),
                Is.EqualTo(adapterCountBefore),
                "An incomplete configuration should fail before any of the five adapter roots is created.");
        }

        // --- Ownership ---

        [UnityTest]
        public IEnumerator Destroying_the_bootstrap_destroys_the_five_separately_rooted_adapters_it_created()
        {
            // Arrange
            Bootstrap sut = InstantiateConfiguredBootstrap();
            GameObject[] adapterRoots =
            {
                FindSingleAdapter<Camera>("MainCamera").gameObject,
                FindSingleAdapter<BoardView>("BoardView").gameObject,
                FindSingleAdapter<ApplicationUI>("ApplicationUI").gameObject,
                FindSingleAdapter<InputService>("InputService").gameObject,
                FindSingleAdapter<DelayScheduler>("DelayScheduler").gameObject,
            };

            // Act
            Object.Destroy(sut.gameObject);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => adapterRoots.All(adapterRoot => adapterRoot == null),
                "Destroying Bootstrap should destroy all five adapter roots it created.");
        }

        [UnityTest]
        public IEnumerator Destroying_the_bootstrap_disposes_the_application_flow_and_board_presenter()
        {
            // Arrange
            (GameObject holder, Bootstrap sut) = CreateInactiveBootstrapFixture();
            RecordingInputService inputAdapterTemplate = new GameObject("InputService").AddComponent<RecordingInputService>();
            TestSerializedReference.AssignPrefab(sut, "_inputServicePrefab", inputAdapterTemplate);

            // Act
            holder.SetActive(true);

            // Assert
            RecordingInputService inputAdapter = InstantiatedAdapter(inputAdapterTemplate);
            Assert.That(inputAdapter.isActiveAndEnabled, Is.True, "The recorded input adapter should be active in the composed graph.");
            Assert.That(inputAdapter.PressedSubscriptionCount, Is.EqualTo(1), "BoardPresenter should listen for board presses on the input adapter.");
            Assert.That(inputAdapter.BackPressedSubscriptionCount, Is.EqualTo(1), "ApplicationFlow should listen for Back on the input adapter.");

            // Release the template so only the adapter Bootstrap owns remains.
            Object.Destroy(inputAdapterTemplate.gameObject);

            // Act
            Object.Destroy(sut.gameObject);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => inputAdapter == null
                    && inputAdapter.PressedSubscriptionCount == 0
                    && inputAdapter.BackPressedSubscriptionCount == 0,
                "Destroying Bootstrap should dispose ApplicationFlow and BoardPresenter so the input adapter they listened to is torn down without handlers.");
            Assert.That(
                inputAdapter.PressedSubscriptionsReleasedBeforeTeardown,
                Is.True,
                "BoardPresenter should release its press subscription before the input adapter is torn down.");
            Assert.That(
                inputAdapter.BackPressedSubscriptionsReleasedBeforeTeardown,
                Is.True,
                "ApplicationFlow should release its Back subscription before the input adapter is torn down.");
        }

        [UnityTest]
        public IEnumerator Input_service_cleanup_stays_safe_when_ownership_and_the_lifecycle_both_request_disposal()
        {
            // Arrange
            Bootstrap bootstrap = InstantiateConfiguredBootstrap();
            InputService sut = FindSingleAdapter<InputService>("InputService");

            // Act
            TestDelegate disposeExplicitly = () => sut.Dispose();
            Assert.That(disposeExplicitly, Throws.Nothing, "An explicit disposal request should be safe.");
            Assert.That(disposeExplicitly, Throws.Nothing, "Repeated disposal should stay safe.");
            Object.Destroy(bootstrap.gameObject);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => sut == null,
                "The input adapter root should still be destroyed with Bootstrap after repeated disposal.");
        }

        // --- Fixtures ---

        private IEnumerable<Component> AdapterComponents()
        {
            return AdaptersInFixtureScene<Camera>().Cast<Component>()
                .Concat(AdaptersInFixtureScene<BoardView>())
                .Concat(AdaptersInFixtureScene<ApplicationUI>())
                .Concat(AdaptersInFixtureScene<InputService>())
                .Concat(AdaptersInFixtureScene<DelayScheduler>());
        }

        private T[] AdaptersInFixtureScene<T>() where T : Component
        {
            return Object
                .FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(adapter => adapter.gameObject.scene == _fixtureScene)
                .ToArray();
        }

        private T FindSingleAdapter<T>(string adapterRole) where T : Component
        {
            T[] adapters = AdaptersInFixtureScene<T>();
            Assert.That(adapters, Has.Length.EqualTo(1), $"Bootstrap should create exactly one {adapterRole} adapter.");

            T adapter = adapters[0];
            Assert.That(adapter.gameObject.name, Is.EqualTo(adapterRole), $"The {adapterRole} adapter should be named for its role without a clone suffix.");
            Assert.That(adapter.transform.parent, Is.Null, $"The {adapterRole} adapter should be a separate scene root.");
            Assert.That(adapter.gameObject.activeInHierarchy, Is.True, $"The {adapterRole} adapter should be active.");
            return adapter;
        }

        private RecordingInputService InstantiatedAdapter(RecordingInputService template)
        {
            RecordingInputService[] instances = AdaptersInFixtureScene<RecordingInputService>()
                .Where(adapter => adapter != template)
                .ToArray();
            Assert.That(instances, Has.Length.EqualTo(1), "Bootstrap should instantiate exactly one recorded input adapter.");
            return instances[0];
        }

        private (GameObject Holder, Bootstrap Sut) CreateInactiveBootstrapFixture()
        {
            GameObject holder = new("Bootstrap lifecycle fixture");
            holder.SetActive(false);

            Bootstrap sut = Object.Instantiate(LoadProductionPrefab(), holder.transform).GetComponent<Bootstrap>();
            Assert.That(sut, Is.Not.Null, "The production Bootstrap prefab should carry its composition root component.");
            return (holder, sut);
        }

        private Bootstrap InstantiateConfiguredBootstrap()
        {
            Bootstrap sut = Object.Instantiate(LoadProductionPrefab()).GetComponent<Bootstrap>();
            Assert.That(sut, Is.Not.Null, "The production Bootstrap prefab should carry its composition root component.");
            return sut;
        }

        private static GameObject LoadProductionPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PRODUCTION_BOOTSTRAP_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, $"The production Bootstrap prefab must exist at {PRODUCTION_BOOTSTRAP_PREFAB_PATH}.");
            return prefab;
        }
    }
}
#endif
