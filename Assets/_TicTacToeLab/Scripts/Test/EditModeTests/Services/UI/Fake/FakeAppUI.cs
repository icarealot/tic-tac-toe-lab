using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeAppUI : IAppUI
    {
        public FakeHomeScreen HomeScreen { get; } = new();
        public FakeBotSelectionScreen BotSelectionScreen { get; } = new();
        public FakeGameplayScreen GameplayScreen { get; } = new();
        public FakeConfirmQuitScreen ConfirmQuitScreen { get; } = new();
        public FakeOutcomeScreen OutcomeScreen { get; } = new();
        public bool HasPopup => ConfirmQuitScreen.IsVisible || OutcomeScreen.IsVisible;
        public IReadOnlyList<string> Operations => _operations;

        private static readonly Dictionary<Type, ScreenLayer> LAYERS_BY_ROLE = new()
        {
            { typeof(IHomeScreen), ScreenLayer.Base },
            { typeof(IBotSelectionScreen), ScreenLayer.Base },
            { typeof(IGameplayScreen), ScreenLayer.Base },
            { typeof(IConfirmQuitScreen), ScreenLayer.Popup },
            { typeof(IOutcomeScreen), ScreenLayer.Popup },
        };

        private readonly List<string> _operations = new();
        private FakeScreen _activeBaseScreen;
        private FakeScreen _activePopupScreen;

        public void Show<TScreen>(Action<TScreen> configure = null) where TScreen : IScreen
        {
            FakeScreen screen = ScreenFor<TScreen>();
            ScreenLayer layer = LAYERS_BY_ROLE[typeof(TScreen)];
            EnsureTransitionAllowed(layer, typeof(TScreen));
            ReplaceActiveScreen(layer);

            _operations.Add($"Show {typeof(TScreen).Name}");
            screen.Present();
            SetActiveScreen(layer, screen);
            configure?.Invoke((TScreen)(object)screen);
        }

        public void Close<TScreen>() where TScreen : IScreen
        {
            FakeScreen screen = ScreenFor<TScreen>();
            ScreenLayer layer = LAYERS_BY_ROLE[typeof(TScreen)];

            if (!ReferenceEquals(ActiveScreenFor(layer), screen))
            {
                return;
            }

            if (layer == ScreenLayer.Base && _activePopupScreen != null)
            {
                throw new InvalidOperationException(
                    $"The fake application UI cannot close the base role {typeof(TScreen).Name} while a popup screen is active.");
            }

            _operations.Add($"Close {typeof(TScreen).Name}");
            ClearActiveScreen(layer);
            screen.Dismiss();
        }

        private FakeScreen ScreenFor<TScreen>() where TScreen : IScreen
        {
            if (typeof(TScreen) == typeof(IHomeScreen))
            {
                return HomeScreen;
            }

            if (typeof(TScreen) == typeof(IBotSelectionScreen))
            {
                return BotSelectionScreen;
            }

            if (typeof(TScreen) == typeof(IGameplayScreen))
            {
                return GameplayScreen;
            }

            if (typeof(TScreen) == typeof(IConfirmQuitScreen))
            {
                return ConfirmQuitScreen;
            }

            if (typeof(TScreen) == typeof(IOutcomeScreen))
            {
                return OutcomeScreen;
            }

            throw new InvalidOperationException(
                $"The fake application UI has no screen double for the role {typeof(TScreen).Name}.");
        }

        private void EnsureTransitionAllowed(ScreenLayer layer, Type role)
        {
            if (layer == ScreenLayer.Popup)
            {
                if (_activeBaseScreen == null)
                {
                    throw new InvalidOperationException(
                        $"The fake application UI cannot show the popup role {role.Name} before a base screen is shown.");
                }

                return;
            }

            if (_activePopupScreen != null)
            {
                throw new InvalidOperationException(
                    $"The fake application UI cannot show the base role {role.Name} while a popup screen is active.");
            }
        }

        private void ReplaceActiveScreen(ScreenLayer layer)
        {
            FakeScreen activeScreen = ActiveScreenFor(layer);

            if (activeScreen == null)
            {
                return;
            }

            activeScreen.Dismiss();
            ClearActiveScreen(layer);
        }

        private FakeScreen ActiveScreenFor(ScreenLayer layer)
        {
            return layer == ScreenLayer.Base ? _activeBaseScreen : _activePopupScreen;
        }

        private void SetActiveScreen(ScreenLayer layer, FakeScreen screen)
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
    }
}
