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
        public IEnumerator The_production_scene_wires_every_long_lived_adapter_and_prefab_dependency_by_role()
        {
            // Arrange
            yield return IE_LoadScene();

            // Assert
            Bootstrap bootstrap = Object.FindFirstObjectByType<Bootstrap>();
            Assert.That(bootstrap, Is.Not.Null, "The production scene should ship its composition root.");

            Camera camera = WiredAdapter<Camera>(bootstrap, "_camera");
            BoardView boardView = WiredAdapter<BoardView>(bootstrap, "_boardView");
            ApplicationUI applicationUI = WiredAdapter<ApplicationUI>(bootstrap, "_applicationUI");
            InputService inputService = WiredAdapter<InputService>(bootstrap, "_inputService");
            DelayScheduler delayScheduler = WiredAdapter<DelayScheduler>(bootstrap, "_delayScheduler");

            Assert.That(camera.isActiveAndEnabled, Is.True, "The wired camera should be usable for board conversion.");
            Assert.That(inputService.isActiveAndEnabled, Is.True, "The wired input adapter should be active.");
            Assert.That(delayScheduler.isActiveAndEnabled, Is.True, "The wired scheduling adapter should be active.");

            Assert.That(
                boardView.GetComponentsInChildren<CellView>(),
                Has.Length.EqualTo(BOARD_CELL_COUNT),
                "The wired board view should construct its cells from its own cell prefab.");
            Assert.That(
                applicationUI.GetComponentInChildren<MainMenuPanel>(includeInactive: true),
                Is.Not.Null,
                "The wired application UI should present its main menu from its own panel prefab.");

            AssertWiredPrefab<CellView>(boardView, "_cellViewPrefab");
            AssertWiredPrefab<MarkView>(boardView.GetComponentInChildren<CellView>(includeInactive: true), "_markViewPrefab");
            AssertWiredPrefab<MainMenuPanel>(applicationUI, "_mainMenuPanelPrefab");
            AssertWiredPrefab<GameplayPanel>(applicationUI, "_gameplayPanelPrefab");
            AssertWiredPrefab<ConfirmQuitPopup>(applicationUI, "_quitConfirmationPopupPrefab");
            AssertWiredPrefab<OutcomePopup>(applicationUI, "_outcomePopupPrefab");

            Canvas canvas = applicationUI.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null, "The production scene should present its UI through a Canvas.");
            Assert.That(canvas.isActiveAndEnabled, Is.True, "The production Canvas should be usable.");
            Assert.That(applicationUI.GetComponent<GraphicRaycaster>(), Is.Not.Null, "The production Canvas should raycast UI presses.");

            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            Assert.That(eventSystems, Has.Length.EqualTo(1), "The production scene should provide exactly one EventSystem.");
            Assert.That(eventSystems[0].isActiveAndEnabled, Is.True, "The production EventSystem should be usable.");
            InputSystemUIInputModule uiInputModule = eventSystems[0].GetComponent<InputSystemUIInputModule>();
            Assert.That(uiInputModule, Is.Not.Null, "The production EventSystem should route Input System UI input.");
            Assert.That(uiInputModule.isActiveAndEnabled, Is.True, "The production EventSystem input module should be usable.");
        }

        private static T WiredAdapter<T>(Bootstrap bootstrap, string fieldName) where T : Component
        {
            T adapter = ReadSerializedReference<T>(bootstrap, fieldName);
            Assert.That(adapter, Is.Not.Null, $"The composition root should wire the {typeof(T).Name} scene adapter into '{fieldName}'.");
            return adapter;
        }

        private static void AssertWiredPrefab<T>(Component owner, string fieldName) where T : Component
        {
            T prefab = ReadSerializedReference<T>(owner, fieldName);
            Assert.That(prefab, Is.Not.Null, $"{owner.GetType().Name} should keep its {typeof(T).Name} prefab dependency in '{fieldName}'.");
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
