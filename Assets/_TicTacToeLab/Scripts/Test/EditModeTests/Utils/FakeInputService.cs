using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeInputService : IInputService
    {
        public event Action<Vector2> Pressed;

        public bool HasSubscribers => Pressed != null;

        public void RaisePress(Vector2 screenPoint)
        {
            Pressed?.Invoke(screenPoint);
        }
    }
}
