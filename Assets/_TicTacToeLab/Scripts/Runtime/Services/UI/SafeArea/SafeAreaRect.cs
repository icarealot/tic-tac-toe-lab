using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaRect : MonoBehaviour
    {
        private RectTransform RectTransformComponent
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;

        private Rect _appliedSafeArea;
        private int _appliedScreenWidth;
        private int _appliedScreenHeight;

        private void Awake()
        {
            ApplyCurrentSafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedSafeArea
                || Screen.width != _appliedScreenWidth
                || Screen.height != _appliedScreenHeight)
            {
                ApplyCurrentSafeArea();
            }
        }

        private void ApplyCurrentSafeArea()
        {
            _appliedSafeArea = Screen.safeArea;
            _appliedScreenWidth = Screen.width;
            _appliedScreenHeight = Screen.height;

            SafeAreaInsets insets = SafeAreaCalculator.Calculate(_appliedSafeArea, _appliedScreenWidth, _appliedScreenHeight);

            RectTransformComponent.anchorMin = insets.AnchorMin;
            RectTransformComponent.anchorMax = insets.AnchorMax;
            RectTransformComponent.offsetMin = Vector2.zero;
            RectTransformComponent.offsetMax = Vector2.zero;
        }
    }
}
