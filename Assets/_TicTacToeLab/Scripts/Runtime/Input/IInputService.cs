using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IInputService
    {
        public event Action<Vector2> Pressed;
        public event Action BackPressed;

        public void EnablePlayerPress();
        public void DisablePlayerPress();
    }
}
