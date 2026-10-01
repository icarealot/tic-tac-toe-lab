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
        public Type LastRequestedRole { get; private set; }
        public Type LastClosedRole { get; private set; }

        private readonly Dictionary<Type, IScreen> _screenDoubles;

        public FakeAppUI()
        {
            _screenDoubles = new Dictionary<Type, IScreen>
            {
                { typeof(IHomeScreen), HomeScreen },
                { typeof(IBotSelectionScreen), BotSelectionScreen },
                { typeof(IGameplayScreen), GameplayScreen },
                { typeof(IConfirmQuitScreen), ConfirmQuitScreen },
                { typeof(IOutcomeScreen), OutcomeScreen },
            };
        }

        public void Show<TScreen>(Action<TScreen> configure = null) where TScreen : IScreen
        {
            TScreen screen = ScreenFor<TScreen>();
            ((FakeScreen)(object)screen).RecordPresentation();
            LastRequestedRole = typeof(TScreen);
            configure?.Invoke(screen);
        }

        public void Close<TScreen>() where TScreen : IScreen
        {
            LastClosedRole = typeof(TScreen);
        }

        private TScreen ScreenFor<TScreen>() where TScreen : IScreen
        {
            return (TScreen)(object)_screenDoubles[typeof(TScreen)];
        }
    }
}
