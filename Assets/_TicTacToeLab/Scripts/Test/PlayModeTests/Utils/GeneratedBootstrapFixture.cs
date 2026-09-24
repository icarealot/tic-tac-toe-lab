#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Generates every long-lived adapter template Bootstrap composes so isolated lifecycle checks never load production assets.
    /// The fixture owns its generated objects and the scene they live in, and restores the previously active scene when it tears down.
    /// </summary>
    public sealed class GeneratedBootstrapFixture
    {
        public GeneratedApplicationUIFixture ApplicationUI { get; }

        private readonly Scene _previousActiveScene;
        private readonly Scene _fixtureScene;
        private readonly List<GameObject> _generatedRoots = new();
        private readonly BoardView _boardViewTemplate;
        private readonly ApplicationUI _applicationUITemplate;
        private readonly Camera _cameraTemplate;
        private readonly InputService _inputServiceTemplate;
        private readonly DelayScheduler _delaySchedulerTemplate;

        public GeneratedBootstrapFixture()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
            _fixtureScene = SceneManager.CreateScene("Generated bootstrap fixture scene");
            _ = SceneManager.SetActiveScene(_fixtureScene);

            GeneratedBoardFixture board = new();
            ApplicationUI = new GeneratedApplicationUIFixture();

            _boardViewTemplate = board.BoardTemplate;
            _applicationUITemplate = ApplicationUI.CreateApplicationUI();
            _cameraTemplate = CreateRoot("Generated camera template").AddComponent<Camera>();
            _inputServiceTemplate = CreateRoot("Generated input service template").AddComponent<InputService>();
            _delaySchedulerTemplate = CreateRoot("Generated delay scheduler template").AddComponent<DelayScheduler>();
        }

        public Bootstrap CreateBootstrapHost()
        {
            GameObject host = CreateRoot("Generated bootstrap host");
            host.SetActive(false);
            return host.AddComponent<Bootstrap>();
        }

        public void AssignAdapterTemplates(Bootstrap bootstrap)
        {
            TestSerializedReference.AssignPrefab(bootstrap, "_cameraPrefab", _cameraTemplate);
            TestSerializedReference.AssignPrefab(bootstrap, "_boardViewPrefab", _boardViewTemplate);
            TestSerializedReference.AssignPrefab(bootstrap, "_applicationUIPrefab", _applicationUITemplate);
            TestSerializedReference.AssignPrefab(bootstrap, "_inputServicePrefab", _inputServiceTemplate);
            TestSerializedReference.AssignPrefab(bootstrap, "_delaySchedulerPrefab", _delaySchedulerTemplate);
        }

        public Bootstrap CreateConfiguredBootstrap()
        {
            Bootstrap bootstrap = CreateBootstrapHost();
            AssignAdapterTemplates(bootstrap);
            bootstrap.gameObject.SetActive(true);
            return bootstrap;
        }

        public Component[] OwnedAdapterRoots()
        {
            return AdaptersInFixtureScene<Camera>().Cast<Component>()
                .Concat(AdaptersInFixtureScene<BoardView>())
                .Concat(AdaptersInFixtureScene<ApplicationUI>())
                .Concat(AdaptersInFixtureScene<InputService>())
                .Concat(AdaptersInFixtureScene<DelayScheduler>())
                .ToArray();
        }

        public IEnumerator IE_DestroyAll()
        {
            if (_fixtureScene.IsValid() && _fixtureScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(_fixtureScene);
                yield return PlayModeWait.IE_WaitUntilOrFail(
                    () => unload.isDone && _generatedRoots.All(root => root == null),
                    "The generated bootstrap scene and every object in it should be destroyed after the test.");
            }

            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                _ = SceneManager.SetActiveScene(_previousActiveScene);
            }
        }

        private IEnumerable<T> AdaptersInFixtureScene<T>() where T : Component
        {
            return Object
                .FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(adapter => adapter.gameObject.scene == _fixtureScene && !IsTemplate(adapter));
        }

        private bool IsTemplate(Component adapter)
        {
            return adapter == _cameraTemplate
                || adapter == _boardViewTemplate
                || adapter == _applicationUITemplate
                || adapter == _inputServiceTemplate
                || adapter == _delaySchedulerTemplate;
        }

        private GameObject CreateRoot(string name)
        {
            GameObject root = new(name);
            _generatedRoots.Add(root);
            return root;
        }
    }
}
#endif
