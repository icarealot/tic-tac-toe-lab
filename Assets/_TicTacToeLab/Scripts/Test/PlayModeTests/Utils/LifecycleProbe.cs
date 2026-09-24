#if UNITY_EDITOR
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Records the creation, visibility, and deferred destruction of a generated window so isolated checks observe
    /// lifecycle outcomes through retained references instead of searching the hierarchy.
    /// </summary>
    public sealed class LifecycleProbe : MonoBehaviour
    {
        public bool WasDestroyed { get; private set; }

        public bool IsShown => !WasDestroyed && gameObject.activeInHierarchy;

        [SerializeField] private GeneratedWindowRegistry _registry;
        private bool _isTemplate;

        public void IgnoreAsTemplate()
        {
            _isTemplate = true;
            if (_registry != null)
            {
                _registry.Unregister(this);
            }
        }

        private void Awake()
        {
            if (_isTemplate || _registry == null)
            {
                return;
            }

            _registry.Register(this);
        }

        private void OnDestroy()
        {
            WasDestroyed = true;
            if (_registry != null)
            {
                _registry.Unregister(this);
            }
        }
    }
}
#endif
