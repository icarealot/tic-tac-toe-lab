using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class UIRoot : MonoBehaviour, IUIRoot
    {
        public RectTransform PanelLayer => _panelLayer;
        public RectTransform PopupLayer => _popupLayer;

        [SerializeField] private RectTransform _panelLayer;
        [SerializeField] private RectTransform _popupLayer;
    }
}
