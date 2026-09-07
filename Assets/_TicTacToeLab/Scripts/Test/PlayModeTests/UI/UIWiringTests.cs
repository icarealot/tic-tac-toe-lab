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
        public IEnumerator The_gameplay_panel_is_present_under_the_panel_layer_at_startup()
        {
            yield return IE_LoadScene();

            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            GameplayPanel gameplayPanel = Object.FindFirstObjectByType<GameplayPanel>();

            Assert.That(gameplayPanel, Is.Not.Null);
            Assert.That(gameplayPanel.transform.parent, Is.EqualTo(uiRoot.PanelLayer));
        }

        [UnityTest]
        public IEnumerator The_turn_display_changes_after_a_press_places_a_mark()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            TMPro.TMP_Text turnText = Object.FindFirstObjectByType<GameplayPanel>().GetComponentInChildren<TMPro.TMP_Text>();
            string turnTextBeforePress = turnText.text;

            yield return IE_PressCell(mouse, GameObject.Find("Cell (0, 0)").transform);

            Assert.That(turnText.text, Is.Not.EqualTo(turnTextBeforePress));
        }
    }
}
#endif
