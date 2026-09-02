using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeCameraService : ICameraService
    {
        public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
        {
            return new Vector3(screenPoint.x, screenPoint.y, 0f);
        }
    }
}
