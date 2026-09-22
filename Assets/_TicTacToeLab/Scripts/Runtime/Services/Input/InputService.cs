using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TicTacToeLab.Runtime
{
    public class InputService : MonoBehaviour, IInputService, IDisposable
    {
        public event Action<Vector2> Pressed;
        public event Action BackPressed;

        private InputSystem_Actions _actions;

        private void Awake()
        {
            _actions = new InputSystem_Actions();
            _actions.Player.Press.performed += OnPressPerformed;
            _actions.Player.Back.performed += OnBackPerformed;
            _actions.Player.Enable();
            _actions.Player.Press.Disable();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_actions == null)
            {
                return;
            }

            _actions.Player.Press.performed -= OnPressPerformed;
            _actions.Player.Back.performed -= OnBackPerformed;
            _actions.Player.Disable();
            _actions.Dispose();
            _actions = null;
        }

        public void EnablePlayerPress()
        {
            if (_actions == null)
            {
                return;
            }

            _actions.Player.Press.Enable();
        }

        public void DisablePlayerPress()
        {
            if (_actions == null)
            {
                return;
            }

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
