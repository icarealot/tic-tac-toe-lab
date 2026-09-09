using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    /// <summary>
    /// Plain EditMode substitute for the consolidated factory. It resolves only the window roles the
    /// UI service requests, records what it creates and returns, and never holds a GameObject.
    /// </summary>
    public sealed class FakeFactoryService : IFactoryService
    {
        public List<FakeMainMenuPanel> MenuPanels { get; } = new();
        public List<FakeGameplayPanel> Panels { get; } = new();
        public List<FakeConfirmQuitPopup> Popups { get; } = new();
        public List<IWindow> ReturnedWindows { get; } = new();

        public T Get<T>() where T : class
        {
            return Get<T>(null);
        }

        public T Get<T>(Transform parent) where T : class
        {
            if (typeof(T) == typeof(IMainMenuPanel))
            {
                FakeMainMenuPanel panel = new();
                MenuPanels.Add(panel);
                return (T)(object)panel;
            }

            if (typeof(T) == typeof(IGameplayPanel))
            {
                FakeGameplayPanel panel = new();
                Panels.Add(panel);
                return (T)(object)panel;
            }

            if (typeof(T) == typeof(IConfirmQuitPopup))
            {
                FakeConfirmQuitPopup popup = new();
                Popups.Add(popup);
                return (T)(object)popup;
            }

            throw new InvalidOperationException($"No fake is registered for role '{typeof(T).Name}'.");
        }

        public void Return<T>(T instance) where T : class
        {
            if (instance is IWindow window)
            {
                ReturnedWindows.Add(window);
            }
        }
    }

    /// <summary>
    /// Plain EditMode substitute for the UI root. The window layers are real RectTransforms in the
    /// production scene, which EditMode deliberately does not hold, so both report null; parenting
    /// itself is verified against production wiring in PlayMode.
    /// </summary>
    public sealed class FakeUIRoot : IUIRoot
    {
        public RectTransform PanelLayer => null;
        public RectTransform PopupLayer => null;
    }

    public abstract class FakeWindow : IWindow
    {
        public bool IsVisible { get; private set; }
        public List<ICoroutineService> ConstructionServices { get; } = new();

        public void Construct(ICoroutineService coroutineService)
        {
            ConstructionServices.Add(coroutineService);
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }
    }

    public sealed class FakeMainMenuPanel : FakeWindow, IMainMenuPanel
    {
        public Action ConfiguredOnStart { get; private set; }

        private Action _onStart;

        public void Setup(Action onStart)
        {
            _onStart = onStart;
            ConfiguredOnStart = onStart;
        }

        public void StartGame()
        {
            _onStart?.Invoke();
        }
    }

    public sealed class FakeGameplayPanel : FakeWindow, IGameplayPanel
    {
        public IBoardSession ConfiguredBoardSession { get; private set; }
        public Action ConfiguredOnBack { get; private set; }

        private Action _onBack;

        public void Setup(IBoardSession boardSession, Action onBack)
        {
            ConfiguredBoardSession = boardSession;
            _onBack = onBack;
            ConfiguredOnBack = onBack;
        }

        public void Back()
        {
            _onBack?.Invoke();
        }
    }

    public sealed class FakeConfirmQuitPopup : FakeWindow, IConfirmQuitPopup
    {
        private Action _onYes;
        private Action _onNo;

        public void Setup(Action onYes, Action onNo)
        {
            _onYes = onYes;
            _onNo = onNo;
        }

        public void Yes()
        {
            _onYes?.Invoke();
        }

        public void No()
        {
            _onNo?.Invoke();
        }
    }
}
