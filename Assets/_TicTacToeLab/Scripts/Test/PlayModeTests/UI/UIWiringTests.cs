#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class UIWiringTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator The_scene_starts_with_one_ui_root_exposing_both_window_layers()
        {
            yield return IE_LoadScene();

            UIRoot[] startupRoots = Object.FindObjectsByType<UIRoot>(FindObjectsSortMode.None);

            Assert.That(startupRoots.Length, Is.EqualTo(1));

            IUIRoot uiRoot = startupRoots[0];
            Assert.That(uiRoot.PanelLayer, Is.Not.Null);
            Assert.That(uiRoot.PopupLayer, Is.Not.Null);
            Assert.That(uiRoot.PanelLayer.GetComponentInParent<Canvas>(), Is.EqualTo(((Component)uiRoot).GetComponent<Canvas>()));
            Assert.That(uiRoot.PopupLayer.GetComponentInParent<Canvas>(), Is.EqualTo(((Component)uiRoot).GetComponent<Canvas>()));
        }

        [UnityTest]
        public IEnumerator The_menu_panel_is_present_under_the_panel_layer_at_startup()
        {
            yield return IE_LoadScene();

            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            MainMenuPanel menuPanel = Object.FindFirstObjectByType<MainMenuPanel>();

            Assert.That(menuPanel, Is.Not.Null);
            Assert.That(menuPanel.transform.parent, Is.EqualTo(uiRoot.PanelLayer));
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator The_turn_display_changes_after_a_press_places_a_mark()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return IE_StartGame(mouse);

            TMPro.TMP_Text turnText = Object.FindFirstObjectByType<GameplayPanel>().GetComponentInChildren<TMPro.TMP_Text>();
            string turnTextBeforePress = turnText.text;

            yield return IE_PressCell(mouse, GameObject.Find("Cell (0, 0)").transform);

            Assert.That(turnText.text, Is.Not.EqualTo(turnTextBeforePress));
        }
    }
}
#endif
