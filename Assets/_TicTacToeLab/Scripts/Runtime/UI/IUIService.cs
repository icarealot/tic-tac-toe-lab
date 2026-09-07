using System;

namespace TicTacToeLab.Runtime
{
    public interface IUIService
    {
        public bool HasPopup { get; }

        public void ShowPanel<TPanel>(Action<TPanel> configure = null) where TPanel : class, IPanel;
        public void ShowPopup<TPopup>(Action<TPopup> configure = null) where TPopup : class, IPopup;
        public bool TryClosePanel();
        public bool TryClosePopup();
        public void CloseAllPopups();
    }
}
