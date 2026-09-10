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
    public sealed class MainMenuWiringTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator The_menu_shows_the_game_title()
        {
            yield return IE_LoadScene();

            MainMenuPanel menuPanel = Object.FindFirstObjectByType<MainMenuPanel>();
            TMPro.TMP_Text titleText = menuPanel.transform.Find("SafeArea/TitleText").GetComponent<TMPro.TMP_Text>();

            Assert.That(menuPanel.IsVisible, Is.True);
            Assert.That(titleText.text, Is.EqualTo("Tic Tac Toe"));
        }

        [UnityTest]
        public IEnumerator Clicking_the_real_start_button_swaps_the_menu_for_the_gameplay_panel()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Button startButton = Object.FindFirstObjectByType<MainMenuPanel>().GetComponentInChildren<Button>();
            yield return IE_ClickButton(mouse, startButton);

            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Null);

            GameplayPanel gameplayPanel = Object.FindFirstObjectByType<GameplayPanel>();
            Assert.That(gameplayPanel, Is.Not.Null);
            Assert.That(gameplayPanel.transform.parent, Is.EqualTo(uiRoot.PanelLayer));
            Assert.That(gameplayPanel.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator The_turn_display_starts_at_x_after_start()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Button startButton = Object.FindFirstObjectByType<MainMenuPanel>().GetComponentInChildren<Button>();
            yield return IE_ClickButton(mouse, startButton);

            GameplayPanel gameplayPanel = Object.FindFirstObjectByType<GameplayPanel>();
            TMPro.TMP_Text turnText = gameplayPanel.GetComponentInChildren<TMPro.TMP_Text>();

            Assert.That(turnText.text, Is.EqualTo("X's turn"));
        }

        [UnityTest]
        public IEnumerator Escape_on_the_menu_produces_no_popup_and_changes_nothing()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();

            yield return IE_PressBack(keyboard);

            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.Null);

            MainMenuPanel menuPanel = Object.FindFirstObjectByType<MainMenuPanel>();
            Assert.That(menuPanel, Is.Not.Null);
            Assert.That(menuPanel.IsVisible, Is.True);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator The_factory_resolves_the_menu_panel_by_its_interface_role()
        {
            yield return IE_LoadScene();

            FactoryService factory = Object.FindFirstObjectByType<FactoryService>();

            IMainMenuPanel panel = factory.Get<IMainMenuPanel>();

            Assert.That(panel, Is.InstanceOf<MainMenuPanel>());

            factory.Return(panel);
            yield return null;
        }
    }
}
#endif
