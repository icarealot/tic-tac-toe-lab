using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeUIService : IUIService
    {
        public bool HasPanel { get; private set; }
        public bool HasPopup { get; private set; }
        public int ShowPopupCount { get; private set; }
        public FakeMainMenuPanel LastMainMenuPanel { get; private set; }
        public FakeGameplayPanel LastGameplayPanel { get; private set; }
        public FakeConfirmQuitPopup LastPopup { get; private set; }
        public FakeOutcomePopup LastOutcomePopup { get; private set; }

        public void ShowPanel<TPanel>(Action<TPanel> configure = null) where TPanel : class, IPanel
        {
            HasPanel = true;

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
            ShowPopupCount++;
            HasPopup = true;

            if (typeof(TPopup) == typeof(IConfirmQuitPopup))
            {
                LastPopup = new FakeConfirmQuitPopup();
                configure?.Invoke((TPopup)(object)LastPopup);
            }

            if (typeof(TPopup) == typeof(IOutcomePopup))
            {
                LastOutcomePopup = new FakeOutcomePopup();
                configure?.Invoke((TPopup)(object)LastOutcomePopup);
            }
        }

        public bool TryClosePanel()
        {
            bool hadPanel = HasPanel;
            HasPanel = false;
            return hadPanel;
        }

        public bool TryClosePopup()
        {
            bool hadPopup = HasPopup;
            HasPopup = false;
            return hadPopup;
        }

        public void CloseAllPopups()
        {
            HasPopup = false;
        }
    }
}
