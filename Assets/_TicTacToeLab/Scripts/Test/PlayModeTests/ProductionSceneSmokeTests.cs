#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ProductionSceneSmokeTests : SceneWiringTests
    {
        private const int BOARD_CELL_COUNT = 9;

        [UnityTest]
        public IEnumerator The_production_scene_instantiates_each_long_lived_adapter_as_a_named_root()
        {
            // Arrange
            yield return IE_LoadScene();

            // Assert
            Bootstrap bootstrap = Object.FindFirstObjectByType<Bootstrap>();
            Assert.That(bootstrap, Is.Not.Null, "The production scene should ship its composition root.");

            AssertWiredPrefab<Camera>(bootstrap, "_cameraPrefab");
            AssertWiredPrefab<BoardView>(bootstrap, "_boardViewPrefab");
            AssertWiredPrefab<ApplicationUI>(bootstrap, "_applicationUIPrefab");
            AssertWiredPrefab<InputService>(bootstrap, "_inputServicePrefab");
            AssertWiredPrefab<DelayScheduler>(bootstrap, "_delaySchedulerPrefab");

            Camera camera = InstantiatedAdapter<Camera>("MainCamera");
            BoardView boardView = InstantiatedAdapter<BoardView>("BoardView");
            ApplicationUI applicationUI = InstantiatedAdapter<ApplicationUI>("ApplicationUI");
            InputService inputService = InstantiatedAdapter<InputService>("InputService");
            DelayScheduler delayScheduler = InstantiatedAdapter<DelayScheduler>("DelayScheduler");

            Assert.That(camera.isActiveAndEnabled, Is.True, "The instantiated camera should be usable for board conversion.");
            Assert.That(inputService.isActiveAndEnabled, Is.True, "The instantiated input adapter should be active.");
            Assert.That(delayScheduler.isActiveAndEnabled, Is.True, "The instantiated scheduling adapter should be active.");

            Assert.That(
                boardView.GetComponentsInChildren<CellView>(),
                Has.Length.EqualTo(BOARD_CELL_COUNT),
                "The instantiated board view should construct its cells from its own cell prefab.");
            Assert.That(
                applicationUI.GetComponentInChildren<MainMenuPanel>(includeInactive: true),
                Is.Not.Null,
                "The instantiated application UI should present its main menu from its own panel prefab.");

            AssertWiredPrefab<CellView>(boardView, "_cellViewPrefab");
            AssertWiredPrefab<MarkView>(boardView.GetComponentInChildren<CellView>(includeInactive: true), "_markViewPrefab");
            AssertWiredPrefab<MainMenuPanel>(applicationUI, "_mainMenuPanelPrefab");
            AssertWiredPrefab<GameplayPanel>(applicationUI, "_gameplayPanelPrefab");
            AssertWiredPrefab<ConfirmQuitPopup>(applicationUI, "_quitConfirmationPopupPrefab");
            AssertWiredPrefab<OutcomePopup>(applicationUI, "_outcomePopupPrefab");

            Canvas canvas = applicationUI.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null, "The instantiated application UI should present through a Canvas.");
            Assert.That(canvas.isActiveAndEnabled, Is.True, "The instantiated Canvas should be usable.");
            Assert.That(applicationUI.GetComponent<GraphicRaycaster>(), Is.Not.Null, "The production Canvas should raycast UI presses.");

            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            Assert.That(eventSystems, Has.Length.EqualTo(1), "The production scene should provide exactly one EventSystem.");
            Assert.That(eventSystems[0].isActiveAndEnabled, Is.True, "The production EventSystem should be usable.");
            InputSystemUIInputModule uiInputModule = eventSystems[0].GetComponent<InputSystemUIInputModule>();
            Assert.That(uiInputModule, Is.Not.Null, "The production EventSystem should route Input System UI input.");
            Assert.That(uiInputModule.isActiveAndEnabled, Is.True, "The production EventSystem input module should be usable.");
        }

        private static T InstantiatedAdapter<T>(string adapterRole) where T : Component
        {
            T[] adapters = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            Assert.That(adapters, Has.Length.EqualTo(1), $"Bootstrap should instantiate exactly one {adapterRole} adapter.");

            T adapter = adapters[0];
            Assert.That(adapter.name, Is.EqualTo(adapterRole), $"The {adapterRole} adapter should keep its role name without a clone suffix.");
            Assert.That(adapter.transform.parent, Is.Null, $"The {adapterRole} adapter should be a separate scene root.");
            return adapter;
        }

        private static void AssertWiredPrefab<T>(Component owner, string fieldName) where T : Component
        {
            T prefab = ReadSerializedReference<T>(owner, fieldName);
            Assert.That(prefab, Is.Not.Null, $"{owner.GetType().Name} should keep its {typeof(T).Name} prefab dependency in '{fieldName}'.");
            Assert.That(
                PrefabUtility.IsPartOfPrefabAsset(prefab),
                Is.True,
                $"The {typeof(T).Name} dependency in '{fieldName}' should be a prefab asset rather than a scene object.");
        }

        private static T ReadSerializedReference<T>(Component owner, string fieldName) where T : Component
        {
            SerializedObject serializedOwner = new(owner);
            SerializedProperty field = serializedOwner.FindProperty(fieldName);
            Assert.That(field, Is.Not.Null, $"{owner.GetType().Name} should declare the serialized field '{fieldName}'.");
            return field.objectReferenceValue as T;
        }
    }
}
#endif
