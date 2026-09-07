using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeUIFactoryService : IUIFactoryService
    {
        public List<FakeGameplayPanel> Panels { get; } = new();
        public List<FakeConfirmQuitPopup> Popups { get; } = new();
        public List<IWindow> ReturnedWindows { get; } = new();

        public TPanel GetPanel<TPanel>() where TPanel : class, IPanel
        {
            if (!typeof(TPanel).IsAssignableFrom(typeof(FakeGameplayPanel)))
            {
                throw new InvalidOperationException($"No fake panel is registered for type '{typeof(TPanel).Name}'.");
            }

            FakeGameplayPanel panel = new();
            Panels.Add(panel);
            return (TPanel)(object)panel;
        }

        public TPopup GetPopup<TPopup>() where TPopup : class, IPopup
        {
            if (!typeof(TPopup).IsAssignableFrom(typeof(FakeConfirmQuitPopup)))
            {
                throw new InvalidOperationException($"No fake popup is registered for type '{typeof(TPopup).Name}'.");
            }

            FakeConfirmQuitPopup popup = new();
            Popups.Add(popup);
            return (TPopup)(object)popup;
        }

        public void Return(IWindow window)
        {
            ReturnedWindows.Add(window);
        }
    }

    public abstract class FakeWindow : IWindow
    {
        public bool IsVisible { get; private set; }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }
    }

    public sealed class FakeGameplayPanel : FakeWindow, IGameplayPanel
    {
        public IBoardSession ConfiguredBoardSession { get; private set; }

        public void Setup(IBoardSession boardSession)
        {
            ConfiguredBoardSession = boardSession;
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
