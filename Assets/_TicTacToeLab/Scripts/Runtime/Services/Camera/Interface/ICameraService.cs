using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface ICameraService
    {
        public Vector3 ScreenToWorldPoint(Vector2 screenPoint);
    }
}
