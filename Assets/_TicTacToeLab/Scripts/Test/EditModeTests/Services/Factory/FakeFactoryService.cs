using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeFactoryService : IFactoryService
    {
        public List<FakeGameplayPanel> Panels { get; } = new();
        public List<FakeConfirmQuitPopup> Popups { get; } = new();
        public List<IWindow> ReturnedWindows { get; } = new();

        public T Get<T>() where T : class
        {
            return Get<T>(null);
        }

        public T Get<T>(Transform parent) where T : class
        {
            if (typeof(T) == typeof(IGameplayPanel))
            {
                FakeGameplayPanel panel = new();
                Panels.Add(panel);
                return (T)(object)panel;
            }

            if (typeof(T) == typeof(IConfirmQuitPopup))
            {
                FakeConfirmQuitPopup popup = new();
                Popups.Add(popup);
                return (T)(object)popup;
            }

            throw new InvalidOperationException($"No fake is registered for role '{typeof(T).Name}'.");
        }

        public void Return<T>(T instance) where T : class
        {
            if (instance is IWindow window)
            {
                ReturnedWindows.Add(window);
            }
        }
    }
}
