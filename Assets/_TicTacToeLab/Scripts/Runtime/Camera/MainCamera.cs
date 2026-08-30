using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class MainCamera : MonoBehaviour
    {
        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }

                return _camera;
            }
        }

        private Camera _camera;
    }
}
