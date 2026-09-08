#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class WindowVisibilityTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_shown_window_is_visible_and_a_covered_window_is_faded_and_stops_receiving_presses()
        {
            yield return IE_LoadScene();

            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            ICoroutineService coroutineService = Object.FindFirstObjectByType<CoroutineService>();
            IFactoryService factoryService = Object.FindFirstObjectByType<FactoryService>();
            UIService uiService = new(factoryService, uiRoot, coroutineService);

            IGameplayPanel firstPanel = null;
            uiService.ShowPanel<IGameplayPanel>(panel => firstPanel = panel);
            IGameplayPanel secondPanel = null;
            uiService.ShowPanel<IGameplayPanel>(panel => secondPanel = panel);

            CanvasGroup firstCanvasGroup = ((Component)firstPanel).GetComponent<CanvasGroup>();
            CanvasGroup secondCanvasGroup = ((Component)secondPanel).GetComponent<CanvasGroup>();

            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(firstPanel, Is.Not.Null); // Covered, not returned: still alive on its stack.
            Assert.That(firstCanvasGroup.alpha, Is.Zero);
            Assert.That(firstCanvasGroup.interactable, Is.False);
            Assert.That(firstCanvasGroup.blocksRaycasts, Is.False);

            Assert.That(secondPanel.IsVisible, Is.True);
            Assert.That(secondCanvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(secondCanvasGroup.interactable, Is.True);
            Assert.That(secondCanvasGroup.blocksRaycasts, Is.True);

            _ = uiService.TryClosePanel();
            _ = uiService.TryClosePanel();
            yield return null;

            // Closed windows are returned to the factory, which destroys them.
            Assert.That((Component)firstPanel == null, Is.True);
            Assert.That((Component)secondPanel == null, Is.True);
        }
    }
}
#endif
