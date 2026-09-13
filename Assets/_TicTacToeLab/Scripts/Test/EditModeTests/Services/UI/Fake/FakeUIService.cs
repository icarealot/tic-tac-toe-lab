using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeUIService : IUIService
    {
        public List<string> Requests { get; } = new();
        public bool HasPopup { get; private set; }
        public int ShowPopupCount { get; private set; }

        public FakeMainMenuPanel LastMainMenuPanel { get; private set; }
        public FakeGameplayPanel LastGameplayPanel { get; private set; }
        public FakeConfirmQuitPopup LastPopup { get; private set; }

        private bool _hasPanel;

        public void ShowPanel<TPanel>(Action<TPanel> configure = null) where TPanel : class, IPanel
        {
            Requests.Add($"ShowPanel<{typeof(TPanel).Name}>");
            _hasPanel = true;

            if (typeof(TPanel) == typeof(IMainMenuPanel))
            {
                LastMainMenuPanel = new FakeMainMenuPanel();
                configure?.Invoke((TPanel)(object)LastMainMenuPanel);
            }

            if (typeof(TPanel) == typeof(IGameplayPanel))
            {
                LastGameplayPanel = new FakeGameplayPanel();
                configure?.Invoke((TPanel)(object)LastGameplayPanel);
            }
        }

        public void ShowPopup<TPopup>(Action<TPopup> configure = null) where TPopup : class, IPopup
        {
            Requests.Add($"ShowPopup<{typeof(TPopup).Name}>");
            ShowPopupCount++;
            HasPopup = true;

            if (typeof(TPopup) == typeof(IConfirmQuitPopup))
            {
                LastPopup = new FakeConfirmQuitPopup();
                configure?.Invoke((TPopup)(object)LastPopup);
            }
        }

        public bool TryClosePanel()
        {
            Requests.Add("TryClosePanel");
            bool hadPanel = _hasPanel;
            _hasPanel = false;
            return hadPanel;
        }

        public bool TryClosePopup()
        {
            Requests.Add("TryClosePopup");
            bool hadPopup = HasPopup;
            HasPopup = false;
            return hadPopup;
        }

        public void CloseAllPopups()
        {
            Requests.Add("CloseAllPopups");
            HasPopup = false;
        }
    }
}
