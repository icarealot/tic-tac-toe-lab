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

            Button noButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>()
                .NoButton();

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

            Button noButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>()
                .NoButton();
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

            Button yesButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>()
                .YesButton();

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
            Button yesButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>()
                .YesButton();
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

        [UnityTest]
        public IEnumerator A_mouse_click_on_the_back_button_opens_the_confirmation_above_the_gameplay_panel()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);

            Button backButton = GameplayBackButton();
            Assert.That(backButton, Is.Not.Null, "The gameplay panel is missing its BackButton.");

            // The button takes the press; its label never competes for it.
            Assert.That(backButton.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(backButton.GetComponentInChildren<TMPro.TMP_Text>().raycastTarget, Is.False);

            yield return IE_ClickButton(mouse, backButton);

            ConfirmQuitPopup popup = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();
            GameplayPanel panel = uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>();

            Assert.That(popup, Is.Not.Null);
            Assert.That(popup.transform.parent, Is.EqualTo(uiRoot.PopupLayer));
            Assert.That(popup.IsVisible, Is.True);

            // The panel is still on its stack and still readable beneath the question.
            Assert.That(panel, Is.Not.Null);
            Assert.That(panel.IsVisible, Is.True);

            // Every popup draws above every panel because the popup layer is the later sibling.
            Assert.That(uiRoot.PopupLayer.GetSiblingIndex(), Is.GreaterThan(uiRoot.PanelLayer.GetSiblingIndex()));
        }

        [UnityTest]
        public IEnumerator The_back_button_sits_top_left_of_the_safe_area_at_its_designed_size_label_and_color()
        {
            yield return IE_LoadScene();

            yield return IE_StartGame(InputSystem.AddDevice<Mouse>());

            Button backButton = GameplayBackButton();
            RectTransform button = backButton.GetComponent<RectTransform>();

            // Anchored to the safe area's top-left corner, inset like the popup's own buttons.
            Assert.That(button.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(button.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(button.pivot, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(button.sizeDelta, Is.EqualTo(new Vector2(140f, 140f)));
            Assert.That(button.anchoredPosition, Is.EqualTo(new Vector2(60f, -60f)));

            Image image = button.GetComponent<Image>();
            Assert.That(image.color, Is.EqualTo(new Color(0.16f, 0.17f, 0.22f, 1f)));

            TMPro.TMP_Text label = button.GetComponentInChildren<TMPro.TMP_Text>();
            Assert.That(label.text, Is.EqualTo("Back"));
            Assert.That(label.fontSize, Is.EqualTo(48f));
        }

        [UnityTest]
        public IEnumerator Answering_no_after_a_back_button_click_leaves_the_game_running_from_the_buttons_path()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);
            Button backButton = GameplayBackButton();

            yield return IE_ClickButton(mouse, backButton);
            Button noButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>()
                .NoButton();
            yield return IE_ClickButton(mouse, noButton);

            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);
            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Not.Null);

            // Play continues, and the button answers again on the same path.
            yield return IE_ClickButton(mouse, backButton);
            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_press_where_the_back_button_sits_while_the_confirmation_is_up_leaves_the_confirmation_up()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);
            Button backButton = GameplayBackButton();
            yield return IE_ClickButton(mouse, backButton);

            ConfirmQuitPopup popup = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();
            Assert.That(popup, Is.Not.Null);

            // The dimmer absorbs the press where the button sits: the question stays up,
            // and no second confirmation can stack on top of the first.
            yield return IE_ClickButton(mouse, backButton);

            ConfirmQuitPopup[] popups = uiRoot.PopupLayer.GetComponentsInChildren<ConfirmQuitPopup>();
            Assert.That(popups.Length, Is.EqualTo(1));
            Assert.That(popups[0].IsVisible, Is.True);

            // Back still closes the question, and the button answers again afterwards.
            yield return IE_PressBack(keyboard);
            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);

            yield return IE_ClickButton(mouse, backButton);
            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_new_game_shows_a_fresh_gameplay_panel_with_the_back_button_again()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_StartGame(mouse);
            Button backButton = GameplayBackButton();
            yield return IE_ClickButton(mouse, backButton);

            Button yesButton = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>()
                .YesButton();
            yield return IE_ClickButton(mouse, yesButton);

            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Null);

            Button startButton = Object.FindFirstObjectByType<MainMenuPanel>().GetComponentInChildren<Button>();
            yield return IE_ClickButton(mouse, startButton);

            GameplayPanel freshPanel = Object.FindFirstObjectByType<GameplayPanel>();
            Assert.That(freshPanel, Is.Not.Null);
            Assert.That(freshPanel.IsVisible, Is.True);
            Assert.That(freshPanel.BackButton(), Is.Not.Null);
        }

        // The back button of the gameplay panel currently on its stack. The panel outlives the
        // popup it opens, so a fetch made before opening the popup stays valid for every click
        // that follows.
        private static Button GameplayBackButton()
        {
            return Object.FindFirstObjectByType<GameplayPanel>().BackButton();
        }
    }
}
#endif
