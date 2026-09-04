using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class UIRoot : MonoBehaviour
    {
        public RectTransform PanelLayer => _panelLayer;
        public RectTransform PopupLayer => _popupLayer;

        [SerializeField] private RectTransform _panelLayer;
        [SerializeField] private RectTransform _popupLayer;

        public void Construct(RectTransform panelLayer, RectTransform popupLayer)
        {
            _panelLayer = panelLayer;
            _popupLayer = popupLayer;
        }
    }
}
