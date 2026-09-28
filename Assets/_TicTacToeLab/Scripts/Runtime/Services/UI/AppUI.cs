using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class AppUI : MonoBehaviour, IAppUI
    {
        [SerializeField] private HomePanel _homePanelPrefab;
        [SerializeField] private GameplayPanel _gameplayPanelPrefab;
        [SerializeField] private ConfirmQuitPopup _quitConfirmationPopupPrefab;
        [SerializeField] private OutcomePopup _outcomePopupPrefab;
        [SerializeField] private List<ScreenRegistration> _screenRegistrations = new();
        [SerializeField] private RectTransform _panelLayer;
        [SerializeField] private RectTransform _popupLayer;

        private readonly Dictionary<Type, ScreenRegistration> _registrationsByRole = new();
        private Component _currentPanel;
        private Component _currentPopup;
        private MonoBehaviour _activeBaseScreen;
        private MonoBehaviour _activePopupScreen;

        private void Awake()
        {
            BuildScreenRegistry();
        }

        public void Show<TScreen>(Action<TScreen> configure = null) where TScreen : IScreen
        {
            ScreenRegistration registration = RequireRegistration<TScreen>();
            EnsureTransitionAllowed(registration.Layer, typeof(TScreen));
            ReplaceActiveScreen(registration.Layer);

            MonoBehaviour screen = Instantiate(registration.Prefab, LayerTransformFor(registration.Layer));
            screen.gameObject.SetActive(true);

            try
            {
                configure?.Invoke((TScreen)(IScreen)screen);
            }
            catch
            {
                RemoveScreen(screen, registration.Layer);
                throw;
            }

            SetActiveScreen(registration.Layer, screen);
        }

        public void Close<TScreen>() where TScreen : IScreen
        {
            ScreenRegistration registration = RequireRegistration<TScreen>();
            MonoBehaviour activeScreen = ActiveScreenFor(registration.Layer);

            if (activeScreen == null || activeScreen is not TScreen)
            {
                return;
            }

            if (registration.Layer == ScreenLayer.Base && _activePopupScreen != null)
            {
                throw new InvalidOperationException(
                    $"The application UI cannot close the active base screen {typeof(TScreen).Name} while a popup screen is active.");
            }

            RemoveScreen(activeScreen, registration.Layer);
        }

        public void ShowHome(Action onStart)
        {
            DestroyCurrentPanel();

            HomePanel panel = Instantiate(_homePanelPrefab, _panelLayer);
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

        private void BuildScreenRegistry()
        {
            _registrationsByRole.Clear();

            foreach (ScreenRegistration registration in _screenRegistrations)
            {
                RegisterScreen(registration);
            }
        }

        private void RegisterScreen(ScreenRegistration registration)
        {
            if (registration.Prefab == null)
            {
                throw new InvalidOperationException(
                    "Every application UI screen registration must reference a screen component prefab, but one registration has no prefab.");
            }

            Type role = FindScreenRole(registration.Prefab.GetType());

            if (_registrationsByRole.ContainsKey(role))
            {
                throw new InvalidOperationException(
                    $"The screen role {role.Name} is registered more than once; each screen role may only have one registration.");
            }

            _registrationsByRole.Add(role, registration);
        }

        private static Type FindScreenRole(Type screenType)
        {
            List<Type> roles = new();

            foreach (Type candidate in screenType.GetInterfaces())
            {
                if (candidate != typeof(IScreen) && typeof(IScreen).IsAssignableFrom(candidate))
                {
                    roles.Add(candidate);
                }
            }

            if (roles.Count == 1)
            {
                return roles[0];
            }

            if (roles.Count == 0)
            {
                throw new InvalidOperationException(
                    $"The registered screen prefab {screenType.Name} implements no screen role interface; each screen must implement exactly one.");
            }

            throw new InvalidOperationException(
                $"The registered screen prefab {screenType.Name} implements more than one screen role interface ({string.Join(", ", roles)}); each screen must implement exactly one.");
        }

        private ScreenRegistration RequireRegistration<TScreen>() where TScreen : IScreen
        {
            if (_registrationsByRole.TryGetValue(typeof(TScreen), out ScreenRegistration registration))
            {
                return registration;
            }

            throw new InvalidOperationException($"The application UI has no registration for the screen role {typeof(TScreen).Name}.");
        }

        private void ReplaceActiveScreen(ScreenLayer layer)
        {
            MonoBehaviour activeScreen = ActiveScreenFor(layer);

            if (activeScreen == null)
            {
                return;
            }

            RemoveScreen(activeScreen, layer);
        }

        private void RemoveScreen(MonoBehaviour screen, ScreenLayer layer)
        {
            ClearActiveScreen(layer);
            screen.gameObject.SetActive(false);
            Destroy(screen.gameObject);
        }

        private void EnsureTransitionAllowed(ScreenLayer layer, Type role)
        {
            if (layer == ScreenLayer.Popup)
            {
                if (_activeBaseScreen == null)
                {
                    throw new InvalidOperationException(
                        $"The application UI cannot show the popup screen {role.Name} before a base screen is shown.");
                }

                return;
            }

            if (_activePopupScreen != null)
            {
                throw new InvalidOperationException(
                    $"The application UI cannot show the base screen {role.Name} while a popup screen is active.");
            }
        }

        private RectTransform LayerTransformFor(ScreenLayer layer)
        {
            return layer == ScreenLayer.Base ? _panelLayer : _popupLayer;
        }

        private MonoBehaviour ActiveScreenFor(ScreenLayer layer)
        {
            return layer == ScreenLayer.Base ? _activeBaseScreen : _activePopupScreen;
        }

        private void SetActiveScreen(ScreenLayer layer, MonoBehaviour screen)
        {
            if (layer == ScreenLayer.Base)
            {
                _activeBaseScreen = screen;
            }
            else
            {
                _activePopupScreen = screen;
            }
        }

        private void ClearActiveScreen(ScreenLayer layer)
        {
            if (layer == ScreenLayer.Base)
            {
                _activeBaseScreen = null;
            }
            else
            {
                _activePopupScreen = null;
            }
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
