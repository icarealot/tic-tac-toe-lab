#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ConfirmQuitPopupTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_popup_reports_yes_and_no_through_the_callbacks_it_was_configured_with()
        {
            yield return IE_LoadScene();

            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            yield return IE_PressBack(keyboard);

            ConfirmQuitPopup popup = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();
            int yesCount = 0;
            int noCount = 0;
            popup.Setup(onYes: () => yesCount++, onNo: () => noCount++);

            popup.Yes();
            popup.No();

            Assert.That(yesCount, Is.EqualTo(1));
            Assert.That(noCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator A_popup_that_is_answered_neither_closes_itself_nor_navigates()
        {
            yield return IE_LoadScene();

            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            yield return IE_PressBack(keyboard);

            ConfirmQuitPopup shownPopup = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();
            shownPopup.Setup(onYes: () => { }, onNo: () => { });

            shownPopup.Yes();
            shownPopup.No();
            yield return null;

            // Answering only reports. Taking the popup off its stack is the asking state's decision.
            Assert.That(uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>(), Is.SameAs(shownPopup));
            Assert.That(shownPopup.IsVisible, Is.True);
            Assert.That(uiRoot.PanelLayer.GetComponentInChildren<GameplayPanel>(), Is.Not.Null);
        }

        private IEnumerator IE_PressBack(Keyboard keyboard)
        {
            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);
            yield return null;
        }
    }
}
#endif
