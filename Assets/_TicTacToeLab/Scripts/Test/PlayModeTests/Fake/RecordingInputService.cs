#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    // Records subscriptions at the IInputService boundary so a test can observe that the
    // application flow and the board presenter release their handlers during teardown.
    public sealed class RecordingInputService : InputService, IInputService
    {
        private readonly List<Action<Vector2>> _pressedHandlers = new();
        private readonly List<Action> _backPressedHandlers = new();

        public int PressedSubscriptionCount => _pressedHandlers.Count;
        public int BackPressedSubscriptionCount => _backPressedHandlers.Count;
        public bool PressedSubscriptionsReleasedBeforeTeardown { get; private set; }
        public bool BackPressedSubscriptionsReleasedBeforeTeardown { get; private set; }

        event Action<Vector2> IInputService.Pressed
        {
            add
            {
                _pressedHandlers.Add(value);
            }

            remove
            {
                _ = _pressedHandlers.Remove(value);
            }
        }

        event Action IInputService.BackPressed
        {
            add
            {
                _backPressedHandlers.Add(value);
            }

            remove
            {
                _ = _backPressedHandlers.Remove(value);
            }
        }

        void IInputService.EnablePlayerPress()
        {
            EnablePlayerPress();
        }

        void IInputService.DisablePlayerPress()
        {
            DisablePlayerPress();
        }

        // Declaring this message hides the private lifecycle cleanup on InputService, so the
        // adapter records the subscription state it was destroyed with and then disposes itself.
        private void OnDestroy()
        {
            PressedSubscriptionsReleasedBeforeTeardown = _pressedHandlers.Count == 0;
            BackPressedSubscriptionsReleasedBeforeTeardown = _backPressedHandlers.Count == 0;
            Dispose();
        }
    }
}
#endif
