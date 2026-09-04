#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class UIWiringTests : SceneWiringTests
    {
        private const string SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";

        [UnityTest]
        public IEnumerator The_gameplay_panel_is_present_under_the_panel_layer_at_startup()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            GameplayPanel gameplayPanel = Object.FindFirstObjectByType<GameplayPanel>();

            Assert.That(gameplayPanel, Is.Not.Null);
            Assert.That(gameplayPanel.transform.parent, Is.EqualTo(uiRoot.PanelLayer));
        }

        [UnityTest]
        public IEnumerator The_turn_display_changes_after_a_press_places_a_mark()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            TMPro.TMP_Text turnText = Object.FindFirstObjectByType<GameplayPanel>().GetComponentInChildren<TMPro.TMP_Text>();
            string turnTextBeforePress = turnText.text;

            yield return IE_PressCell(mouse, GameObject.Find("Cell (0, 0)").transform);

            Assert.That(turnText.text, Is.Not.EqualTo(turnTextBeforePress));
        }

        private IEnumerator IE_PressCell(Mouse mouse, Transform cellTransform)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(cellTransform.position);

            Set(mouse.position, new Vector2(screenPoint.x, screenPoint.y));
            Press(mouse.leftButton);
            Release(mouse.leftButton);

            yield return null;
        }
    }
}
#endif
