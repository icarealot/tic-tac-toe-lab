using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TicTacToeLab.Runtime
{
    public class InputService : IInputService, IDisposable
    {
        public event Action<Vector2> Pressed;
        public event Action BackPressed;

        private readonly InputSystem_Actions _actions;

        public InputService(InputSystem_Actions actions)
        {
            _actions = actions;
            _actions.Player.Press.performed += OnPressPerformed;
            _actions.Player.Back.performed += OnBackPerformed;
            _actions.Player.Enable();
        }

        public void Dispose()
        {
            _actions.Player.Press.performed -= OnPressPerformed;
            _actions.Player.Back.performed -= OnBackPerformed;
            _actions.Player.Disable();
        }

        public void EnablePlayerPress()
        {
            _actions.Player.Press.Enable();
        }

        public void DisablePlayerPress()
        {
            _actions.Player.Press.Disable();
        }

        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            Pressed?.Invoke(_actions.Player.Position.ReadValue<Vector2>());
        }

        private void OnBackPerformed(InputAction.CallbackContext context)
        {
            BackPressed?.Invoke();
        }
    }
}
