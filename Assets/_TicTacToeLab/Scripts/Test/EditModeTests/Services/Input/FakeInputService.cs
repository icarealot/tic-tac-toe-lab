using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeInputService : IInputService
    {
        public event Action<Vector2> Pressed;
        public event Action BackPressed;

        public bool HasSubscribers => Pressed != null;

        public bool IsPlayerPressEnabled { get; private set; } = true;

        public void RaisePress(Vector2 screenPoint)
        {
            Pressed?.Invoke(screenPoint);
        }

        public void RaiseBack()
        {
            BackPressed?.Invoke();
        }

        public void EnablePlayerPress()
        {
            IsPlayerPressEnabled = true;
        }

        public void DisablePlayerPress()
        {
            IsPlayerPressEnabled = false;
        }
    }
}
