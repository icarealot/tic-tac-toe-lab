using System.Collections;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaRect : MonoBehaviour
    {
        private const float POLL_INTERVAL_SECONDS = 1f;

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

        private CoroutineHandle _pollHandle;

        public void Construct(ICoroutineService coroutineService)
        {
            Apply();
            _pollHandle = coroutineService.Run(IE_PollSafeArea());
        }

        private void OnDestroy()
        {
            _pollHandle?.Dispose();
        }

        private IEnumerator IE_PollSafeArea()
        {
            WaitForSeconds wait = new(POLL_INTERVAL_SECONDS);

            while (true)
            {
                yield return wait;
                Apply();
            }
        }

        private void Apply()
        {
            SafeAreaInsets insets = SafeAreaCalculator.Calculate(Screen.safeArea, Screen.width, Screen.height);

            RectTransformComponent.anchorMin = insets.AnchorMin;
            RectTransformComponent.anchorMax = insets.AnchorMax;
            RectTransformComponent.offsetMin = Vector2.zero;
            RectTransformComponent.offsetMax = Vector2.zero;
        }
    }
}
