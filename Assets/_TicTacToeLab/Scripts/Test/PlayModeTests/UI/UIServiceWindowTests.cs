#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class UIServiceWindowTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator Showing_a_panel_through_the_ui_service_parents_it_under_the_panel_layer()
        {
            yield return IE_LoadScene();

            UIService uiService = CreateUIService(out UIRoot uiRoot, out _);
            IGameplayPanel panel = null;
            uiService.ShowPanel<IGameplayPanel>(created => panel = created);

            Assert.That(((Component)panel).transform.parent, Is.EqualTo(uiRoot.PanelLayer));

            _ = uiService.TryClosePanel();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Showing_a_popup_through_the_ui_service_parents_it_under_the_popup_layer()
        {
            yield return IE_LoadScene();

            UIService uiService = CreateUIService(out UIRoot uiRoot, out _);
            IConfirmQuitPopup popup = null;
            uiService.ShowPopup<IConfirmQuitPopup>(created => popup = created);

            Assert.That(((Component)popup).transform.parent, Is.EqualTo(uiRoot.PopupLayer));

            _ = uiService.TryClosePopup();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Showing_a_window_runs_its_safe_area_construction_through_the_coroutine_service()
        {
            yield return IE_LoadScene();

            UIService uiService = CreateUIService(out _, out RecordingCoroutineService coroutineService);

            uiService.ShowPanel<IGameplayPanel>();
            Assert.That(coroutineService.RunCount, Is.EqualTo(1));

            uiService.ShowPopup<IConfirmQuitPopup>();
            Assert.That(coroutineService.RunCount, Is.EqualTo(2));

            _ = uiService.TryClosePopup();
            _ = uiService.TryClosePanel();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Closing_a_window_through_the_ui_service_destroys_the_instance_the_factory_returned()
        {
            yield return IE_LoadScene();

            UIService uiService = CreateUIService(out _, out _);
            IGameplayPanel panel = null;
            uiService.ShowPanel<IGameplayPanel>(created => panel = created);
            Component panelComponent = (Component)panel;

            Assert.That(uiService.TryClosePanel(), Is.True);
            yield return null;

            Assert.That(panelComponent == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Requesting_a_window_role_without_a_registered_prefab_throws()
        {
            yield return IE_LoadScene();

            UIService uiService = CreateUIService(out _, out _);

            Assert.That(() => uiService.ShowPanel<IUnregisteredPanel>(), Throws.TypeOf<InvalidOperationException>());
        }

        private interface IUnregisteredPanel : IPanel
        {
        }

        private UIService CreateUIService(out UIRoot uiRoot, out RecordingCoroutineService coroutineService)
        {
            IFactoryService factoryService = UnityEngine.Object.FindFirstObjectByType<FactoryService>();
            uiRoot = UnityEngine.Object.FindFirstObjectByType<UIRoot>();
            coroutineService = new RecordingCoroutineService();
            return new UIService(factoryService, uiRoot, coroutineService);
        }
    }
}
#endif
