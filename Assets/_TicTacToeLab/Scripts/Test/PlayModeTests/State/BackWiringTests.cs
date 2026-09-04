#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BackWiringTests : SceneWiringTests
    {
        private const string SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";

        [UnityTest]
        public IEnumerator A_keyboard_escape_in_the_real_scene_opens_the_confirmation_above_the_gameplay_panel()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_PressBack(keyboard);

            ConfirmQuitPopup popup = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();
            GameplayPanel panel = uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>();

            Assert.That(popup, Is.Not.Null);
            Assert.That(popup.transform.parent, Is.EqualTo(uiRoot.PopupLayer));
            Assert.That(popup.IsVisible, Is.True);

            // The panel is still on its stack and still readable underneath the question.
            Assert.That(panel, Is.Not.Null);
            Assert.That(panel.IsVisible, Is.True);

            // Every popup draws above every panel because the popup layer is the later sibling.
            Assert.That(uiRoot.PopupLayer.GetSiblingIndex(), Is.GreaterThan(uiRoot.PanelLayer.GetSiblingIndex()));
        }

        [UnityTest]
        public IEnumerator A_second_escape_in_the_real_scene_closes_the_confirmation_and_leaves_the_gameplay_panel()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_PressBack(keyboard);
            yield return IE_PressBack(keyboard);

            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);
            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_mouse_click_on_the_confirmations_no_button_closes_it_and_leaves_the_game_running()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_PressBack(keyboard);

            Button noButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>().transform
                .Find("SafeArea/Dialog/NoButton").GetComponent<Button>();

            yield return IE_ClickButton(mouse, noButton);

            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);
            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_press_on_the_board_places_no_mark_while_the_confirmation_is_up_and_places_one_again_after_answering_no()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Transform cellTransform = GameObject.Find("Cell (0, 0)").transform;

            yield return IE_PressBack(keyboard);
            yield return IE_PressCell(mouse, cellTransform);

            Assert.That(cellTransform.GetComponentInChildren<MarkView>(), Is.Null);

            Button noButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>().transform
                .Find("SafeArea/Dialog/NoButton").GetComponent<Button>();
            yield return IE_ClickButton(mouse, noButton);
            yield return IE_PressCell(mouse, cellTransform);

            Assert.That(cellTransform.GetComponentInChildren<MarkView>(), Is.Not.Null);
        }

        private IEnumerator IE_PressCell(Mouse mouse, Transform cellTransform)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(cellTransform.position);

            Set(mouse.position, new Vector2(screenPoint.x, screenPoint.y));
            Press(mouse.leftButton);
            Release(mouse.leftButton);

            yield return null;
        }

        private IEnumerator IE_ClickButton(Mouse mouse, Button button)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);

            Set(mouse.position, screenPoint);
            yield return null;
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);

            // The button raises onClick on release, and the closed popup is destroyed at end of frame.
            yield return null;
            yield return null;
        }

        private IEnumerator IE_PressBack(Keyboard keyboard)
        {
            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);

            // The closed window is destroyed at the end of the frame, so let it go by.
            yield return null;
        }
    }
}
#endif
