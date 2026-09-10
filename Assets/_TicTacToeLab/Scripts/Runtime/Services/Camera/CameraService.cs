using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class CameraService : ICameraService
    {
        private readonly Camera _camera;

        public CameraService(Camera camera)
        {
            _camera = camera;
        }

        public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
        {
            return _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, _camera.nearClipPlane));
        }
    }
}
