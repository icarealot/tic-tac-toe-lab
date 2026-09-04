using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeUIRoot : IUIRoot
    {
        public RectTransform PanelLayer { get; }
        public RectTransform PopupLayer { get; }

        public FakeUIRoot()
        {
            GameObject rootObject = new("UIRoot");

            PanelLayer = CreateLayer("PanelLayer", rootObject.transform);
            PopupLayer = CreateLayer("PopupLayer", rootObject.transform);
        }

        private RectTransform CreateLayer(string layerName, Transform parent)
        {
            RectTransform layer = new GameObject(layerName, typeof(RectTransform)).GetComponent<RectTransform>();
            layer.SetParent(parent);
            return layer;
        }
    }
}
