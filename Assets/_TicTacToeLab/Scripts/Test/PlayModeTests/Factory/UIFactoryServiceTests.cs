#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class UIFactoryServiceTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator The_panel_getter_resolves_a_panel_interface_under_the_panel_layer_and_constructs_it()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            UIFactoryService uiFactory = CreateUIFactory(out UIRoot uiRoot, out RecordingCoroutineService coroutineService);
            IGameplayPanel panel = uiFactory.GetPanel<IGameplayPanel>();

            Assert.That(((Component)panel).transform.parent, Is.EqualTo(uiRoot.PanelLayer));
            Assert.That(coroutineService.RunCount, Is.EqualTo(1));

            uiFactory.Return(panel);
        }

        [UnityTest]
        public IEnumerator The_popup_getter_resolves_a_popup_interface_under_the_popup_layer()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            UIFactoryService uiFactory = CreateUIFactory(out UIRoot uiRoot, out RecordingCoroutineService coroutineService);
            IConfirmQuitPopup popup = uiFactory.GetPopup<IConfirmQuitPopup>();

            Assert.That(((Component)popup).transform.parent, Is.EqualTo(uiRoot.PopupLayer));
            Assert.That(coroutineService.RunCount, Is.EqualTo(1));

            uiFactory.Return(popup);
        }

        [UnityTest]
        public IEnumerator Returning_a_window_destroys_the_created_instance()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            UIFactoryService uiFactory = CreateUIFactory(out _, out _);
            IGameplayPanel panel = uiFactory.GetPanel<IGameplayPanel>();
            Component panelComponent = (Component)panel;

            uiFactory.Return(panel);
            yield return null;

            Assert.That(panelComponent == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Requesting_a_window_without_a_registered_prefab_throws()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            UIFactoryService uiFactory = CreateUIFactory(out _, out _);

            Assert.That(() => uiFactory.GetPanel<IUnregisteredPanel>(), Throws.TypeOf<InvalidOperationException>());
        }

        private UIFactoryService CreateUIFactory(out UIRoot uiRoot, out RecordingCoroutineService coroutineService)
        {
            ComponentFactoryService componentFactoryService = UnityEngine.Object.FindFirstObjectByType<ComponentFactoryService>();
            uiRoot = UnityEngine.Object.FindFirstObjectByType<UIRoot>();
            coroutineService = new RecordingCoroutineService();
            return new UIFactoryService(componentFactoryService, uiRoot, coroutineService);
        }

        private interface IUnregisteredPanel : IPanel
        {
        }

        private sealed class RecordingCoroutineService : ICoroutineService
        {
            public int RunCount { get; private set; }
            public int RunAfterCount { get; private set; }

            public CoroutineHandle Run(IEnumerator routine)
            {
                RunCount++;
                return new CoroutineHandle(() => { });
            }

            public CoroutineHandle RunAfter(float delaySeconds, Action callback)
            {
                RunAfterCount++;
                return new CoroutineHandle(() => { });
            }
        }
    }
}
#endif
