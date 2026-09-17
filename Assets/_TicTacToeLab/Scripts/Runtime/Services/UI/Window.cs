using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Window : MonoBehaviour, IWindow
    {
        public bool IsVisible => CanvasGroupComponent.alpha > 0f;

        private CanvasGroup CanvasGroupComponent
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }
        private CanvasGroup _canvasGroup;

        public void Show()
        {
            CanvasGroupComponent.alpha = 1f;
            CanvasGroupComponent.interactable = true;
            CanvasGroupComponent.blocksRaycasts = true;
        }

        public void Hide()
        {
            CanvasGroupComponent.alpha = 0f;
            CanvasGroupComponent.interactable = false;
            CanvasGroupComponent.blocksRaycasts = false;
        }
    }
}
