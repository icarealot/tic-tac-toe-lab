using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class ApplicationUI : MonoBehaviour, IApplicationUI
    {
        [SerializeField] private MainMenuPanel _mainMenuPanelPrefab;
        [SerializeField] private GameplayPanel _gameplayPanelPrefab;
        [SerializeField] private ConfirmQuitPopup _quitConfirmationPopupPrefab;
        [SerializeField] private OutcomePopup _outcomePopupPrefab;
        [SerializeField] private RectTransform _panelLayer;
        [SerializeField] private RectTransform _popupLayer;

        private Component _currentPanel;
        private Component _currentPopup;

        public void ShowMainMenu(Action onStart)
        {
            DestroyCurrentPanel();

            MainMenuPanel panel = Instantiate(_mainMenuPanelPrefab, _panelLayer);
            panel.Setup(onStart);
            _currentPanel = panel;
        }

        public void ShowGameplay(BoardPresenter boardPresenter, Action onBack)
        {
            DestroyCurrentPanel();

            GameplayPanel panel = Instantiate(_gameplayPanelPrefab, _panelLayer);
            panel.Setup(boardPresenter, onBack);
            _currentPanel = panel;
        }

        public void ShowQuitConfirmation(Action onQuit, Action onCancel)
        {
            DestroyCurrentPopup();

            ConfirmQuitPopup popup = Instantiate(_quitConfirmationPopupPrefab, _popupLayer);
            popup.Setup(onQuit, onCancel);
            _currentPopup = popup;
        }

        public void CloseQuitConfirmation()
        {
            DestroyCurrentPopup();
        }

        public void ShowOutcome(Outcome outcome, Action onContinue)
        {
            DestroyCurrentPopup();

            OutcomePopup popup = Instantiate(_outcomePopupPrefab, _popupLayer);
            popup.Setup(outcome, onContinue);
            _currentPopup = popup;
        }

        public void ClosePopup()
        {
            DestroyCurrentPopup();
        }

        private void DestroyCurrentPanel()
        {
            if (_currentPanel == null)
            {
                return;
            }

            Destroy(_currentPanel.gameObject);
            _currentPanel = null;
        }

        private void DestroyCurrentPopup()
        {
            if (_currentPopup == null)
            {
                return;
            }

            Destroy(_currentPopup.gameObject);
            _currentPopup = null;
        }
    }
}
