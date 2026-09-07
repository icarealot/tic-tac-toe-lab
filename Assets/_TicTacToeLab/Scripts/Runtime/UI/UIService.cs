using System;
using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public class UIService : IUIService
    {
        public bool HasPopup => _popupStack.Count > 0;

        private readonly IUIFactoryService _uiFactoryService;
        private readonly Stack<IPanel> _panelStack = new();
        private readonly Stack<IPopup> _popupStack = new();

        public UIService(IUIFactoryService uiFactoryService)
        {
            _uiFactoryService = uiFactoryService;
        }

        public void ShowPanel<TPanel>(Action<TPanel> configure = null) where TPanel : class, IPanel
        {
            if (HasPopup)
            {
                throw new InvalidOperationException($"Cannot show {typeof(TPanel).Name} while a popup is up. Close the popups first.");
            }

            Show(_panelStack, _uiFactoryService.GetPanel<TPanel>, configure);
        }

        public void ShowPopup<TPopup>(Action<TPopup> configure = null) where TPopup : class, IPopup
        {
            Show(_popupStack, _uiFactoryService.GetPopup<TPopup>, configure);
        }

        public bool TryClosePanel()
        {
            return TryClose(_panelStack);
        }

        public bool TryClosePopup()
        {
            return TryClose(_popupStack);
        }

        public void CloseAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                _ = TryClosePopup();
            }
        }

        private static void Show<TWindow, TShown>(Stack<TWindow> stack, Func<TShown> getWindow, Action<TShown> configure)
            where TWindow : IWindow
            where TShown : class, TWindow
        {
            if (stack.Count > 0)
            {
                stack.Peek().Hide();
            }

            TShown window = getWindow();
            window.Hide();
            configure?.Invoke(window);
            window.Show();

            stack.Push(window);
        }

        private bool TryClose<TWindow>(Stack<TWindow> stack) where TWindow : IWindow
        {
            if (stack.Count == 0)
            {
                return false;
            }

            TWindow topWindow = stack.Pop();
            _uiFactoryService.Return(topWindow);

            if (stack.Count > 0)
            {
                stack.Peek().Show();
            }

            return true;
        }
    }
}
