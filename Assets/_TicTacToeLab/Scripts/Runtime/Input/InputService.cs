using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TicTacToeLab.Runtime
{
    public class InputService : IInputService, IDisposable
    {
        public event Action<Vector2> Pressed;

        private readonly InputSystem_Actions _actions;

        public InputService(InputSystem_Actions actions)
        {
            _actions = actions;
            _actions.Player.Press.performed += OnPressPerformed;
            _actions.Player.Enable();
        }

        public void Dispose()
        {
            _actions.Player.Press.performed -= OnPressPerformed;
            _actions.Player.Disable();
        }

        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            Pressed?.Invoke(_actions.Player.Position.ReadValue<Vector2>());
        }
    }
}
