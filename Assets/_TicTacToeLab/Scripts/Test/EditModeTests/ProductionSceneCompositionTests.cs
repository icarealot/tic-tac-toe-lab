using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TicTacToeLab.EditModeTests
{
    public sealed class ProductionSceneCompositionTests
    {
        private const string PRODUCTION_SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";
        private const string BOOTSTRAP_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/Bootstrap.prefab";
        private const string APPLICATION_UI_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/ApplicationUI.prefab";

        public enum PrefabDivergence
        {
            None,
            ComponentPropertyOverride,
            AddedComponent,
            RemovedComponent,
            AddedChildObject,
            RemovedChildObject,
        }

        private Scene _openedProductionScene;
        private Scene _fixtureScene;

        [TearDown]
        public void Restore_editor_scene_state()
        {
            if (_openedProductionScene.IsValid() && _openedProductionScene.isLoaded)
            {
                _ = EditorSceneManager.CloseScene(_openedProductionScene, removeScene: true);
                _openedProductionScene = default;
            }

            if (_fixtureScene.IsValid() && _fixtureScene.isLoaded)
            {
                _ = EditorSceneManager.ClosePreviewScene(_fixtureScene);
                _fixtureScene = default;
            }
        }

        // --- Authored production scene ---

        [Test]
        public void The_saved_production_scene_contains_only_an_unmodified_bootstrap_prefab_instance()
        {
            // Arrange
            Scene productionScene = OpenSavedProductionScene();

            // Act
            GameObject[] roots = productionScene.GetRootGameObjects();

            // Assert
            Assert.That(
                roots,
                Has.Length.EqualTo(1),
                "The production scene should author exactly one root because Bootstrap owns the runtime composition.");
            GameObject sut = roots[0];
            Assert.That(
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sut),
                Is.EqualTo(BOOTSTRAP_PREFAB_PATH),
                "The production scene root should be an instance of the Bootstrap prefab.");
            Assert.That(
                HasSceneSpecificOverride(sut),
                Is.False,
                "The Bootstrap instance should keep the prefab configuration because the prefab is the single serialized composition definition.");
        }

        // --- Composition override guard ---

        [Test]
        public void An_unmodified_prefab_instance_has_no_scene_specific_override()
        {
            // Arrange
            GameObject sut = InstantiateFixture(PrefabDivergence.None);

            // Act
            bool hasOverride = HasSceneSpecificOverride(sut);

            // Assert
            Assert.That(
                hasOverride,
                Is.False,
                "Unity's default scene-instance metadata should not be mistaken for a composition override.");
        }

        [TestCase(PrefabDivergence.ComponentPropertyOverride)]
        [TestCase(PrefabDivergence.AddedComponent)]
        [TestCase(PrefabDivergence.RemovedComponent)]
        [TestCase(PrefabDivergence.AddedChildObject)]
        [TestCase(PrefabDivergence.RemovedChildObject)]
        public void A_prefab_instance_with_a_scene_specific_divergence_is_rejected(PrefabDivergence divergence)
        {
            // Arrange
            GameObject sut = InstantiateFixture(divergence);

            // Act
            bool hasOverride = HasSceneSpecificOverride(sut);

            // Assert
            Assert.That(
                hasOverride,
                Is.True,
                $"The production scene guard should reject a prefab instance with a scene-specific '{divergence}'.");
        }

        // Unity always records default scene metadata for a placed instance, so only non-default
        // overrides, added or removed components, and added or removed child objects may count as a
        // scene-specific composition definition.
        private static bool HasSceneSpecificOverride(GameObject prefabInstanceRoot)
        {
            return PrefabUtility.HasPrefabInstanceAnyOverrides(prefabInstanceRoot, includeDefaultOverrides: false);
        }

        // --- Fixtures ---

        private Scene OpenSavedProductionScene()
        {
            Scene productionScene = SceneManager.GetSceneByPath(PRODUCTION_SCENE_PATH);
            if (productionScene.IsValid() && productionScene.isLoaded)
            {
                return productionScene;
            }

            _openedProductionScene = EditorSceneManager.OpenScene(PRODUCTION_SCENE_PATH, OpenSceneMode.Additive);
            return _openedProductionScene;
        }

        private GameObject InstantiateFixture(PrefabDivergence divergence)
        {
            _fixtureScene = EditorSceneManager.NewPreviewScene();
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(FixturePrefab(divergence), _fixtureScene);
            ApplyDivergence(divergence, instance);
            return instance;
        }

        private static GameObject FixturePrefab(PrefabDivergence divergence)
        {
            // Only the application UI prefab authors child objects, so it provides the removed-child fixture.
            string prefabPath = divergence == PrefabDivergence.RemovedChildObject ? APPLICATION_UI_PREFAB_PATH : BOOTSTRAP_PREFAB_PATH;
            return LoadPrefab(prefabPath);
        }

        private static GameObject LoadPrefab(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, $"The fixture prefab should exist at {prefabPath}.");
            return prefab;
        }

        private static void ApplyDivergence(PrefabDivergence divergence, GameObject instance)
        {
            switch (divergence)
            {
                case PrefabDivergence.ComponentPropertyOverride:
                    OverrideCameraPrefabReference(instance);
                    break;
                case PrefabDivergence.AddedComponent:
                    _ = instance.AddComponent<BoxCollider>();
                    break;
                case PrefabDivergence.RemovedComponent:
                    Object.DestroyImmediate(instance.GetComponent<Bootstrap>());
                    break;
                case PrefabDivergence.AddedChildObject:
                    _ = PrefabUtility.InstantiatePrefab(LoadPrefab(APPLICATION_UI_PREFAB_PATH), instance.transform);
                    break;
                case PrefabDivergence.RemovedChildObject:
                    Object.DestroyImmediate(instance.transform.Find("PanelLayer").gameObject);
                    break;
                case PrefabDivergence.None:
                    break;
            }
        }

        private static void OverrideCameraPrefabReference(GameObject instance)
        {
            SerializedObject bootstrap = new(instance.GetComponent<Bootstrap>());
            SerializedProperty cameraPrefab = bootstrap.FindProperty("_cameraPrefab");
            Assert.That(cameraPrefab, Is.Not.Null, "The Bootstrap component should declare the serialized field '_cameraPrefab'.");

            cameraPrefab.objectReferenceValue = null;
            _ = bootstrap.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
