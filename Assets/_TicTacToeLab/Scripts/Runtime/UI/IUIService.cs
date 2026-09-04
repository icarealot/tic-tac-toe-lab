using System;

namespace TicTacToeLab.Runtime
{
    public interface IUIService
    {
        public bool HasPopup { get; }

        public void ShowPanel<TPanel>(Action<TPanel> configure = null) where TPanel : Panel;
        public void ShowPopup<TPopup>(Action<TPopup> configure = null) where TPopup : Popup;
        public bool TryClosePanel();
        public bool TryClosePopup();
        public void CloseAllPopups();
    }
}
