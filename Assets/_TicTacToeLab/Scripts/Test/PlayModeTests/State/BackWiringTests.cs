#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BackWiringTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_keyboard_escape_in_the_real_scene_opens_the_confirmation_above_the_gameplay_panel()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);
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
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);

            yield return IE_PressBack(keyboard);
            yield return IE_PressBack(keyboard);

            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);
            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_mouse_click_on_the_confirmations_no_button_closes_it_and_leaves_the_game_running()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);
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
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Transform cellTransform = GameObject.Find("Cell (0, 0)").transform;

            yield return IE_StartGame(mouse);
            yield return IE_PressBack(keyboard);
            yield return IE_PressCell(mouse, cellTransform);

            Assert.That(cellTransform.GetComponentInChildren<MarkView>(), Is.Null);

            Button noButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>().transform
                .Find("SafeArea/Dialog/NoButton").GetComponent<Button>();
            yield return IE_ClickButton(mouse, noButton);
            yield return IE_PressCell(mouse, cellTransform);

            Assert.That(cellTransform.GetComponentInChildren<MarkView>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_mouse_click_on_the_confirmations_yes_button_returns_to_the_menu_panel()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);
            yield return IE_PressBack(keyboard);

            Button yesButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>().transform
                .Find("SafeArea/Dialog/YesButton").GetComponent<Button>();

            yield return IE_ClickButton(mouse, yesButton);

            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);
            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Null);

            MainMenuPanel menuPanel = Object.FindFirstObjectByType<MainMenuPanel>();
            Assert.That(menuPanel, Is.Not.Null);
            Assert.That(menuPanel.transform.parent, Is.EqualTo(uiRoot.PanelLayer));
            Assert.That(menuPanel.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator A_game_abandoned_through_yes_keeps_its_marks_and_start_begins_an_empty_board()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Transform cellTransform = GameObject.Find("Cell (0, 0)").transform;

            yield return IE_StartGame(mouse);
            yield return IE_PressCell(mouse, cellTransform);

            yield return IE_PressBack(keyboard);
            Button yesButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>().transform
                .Find("SafeArea/Dialog/YesButton").GetComponent<Button>();
            yield return IE_ClickButton(mouse, yesButton);

            // The abandoned board keeps its mark, hidden behind the menu.
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Not.Null);
            Assert.That(cellTransform.GetComponentInChildren<MarkView>(), Is.Not.Null);

            Button startButton = Object.FindFirstObjectByType<MainMenuPanel>().GetComponentInChildren<Button>();
            yield return IE_ClickButton(mouse, startButton);

            MarkView[] remainingMarks = Object.FindObjectsByType<MarkView>(FindObjectsSortMode.None);
            Assert.That(remainingMarks, Is.Empty);

            GameplayPanel gameplayPanel = Object.FindFirstObjectByType<GameplayPanel>();
            TMPro.TMP_Text turnText = gameplayPanel.GetComponentInChildren<TMPro.TMP_Text>();
            Assert.That(turnText.text, Is.EqualTo("X's turn"));
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
