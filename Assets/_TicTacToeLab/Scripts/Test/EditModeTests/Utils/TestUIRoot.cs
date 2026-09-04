using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    /// <summary>Builds a bare root with its two layers, standing in for the authored prefab.</summary>
    public static class TestUIRoot
    {
        public static UIRoot Create()
        {
            GameObject rootObject = new("UIRoot");
            UIRoot uiRoot = rootObject.AddComponent<UIRoot>();

            RectTransform panelLayer = new GameObject("PanelLayer", typeof(RectTransform)).GetComponent<RectTransform>();
            panelLayer.SetParent(rootObject.transform);
            RectTransform popupLayer = new GameObject("PopupLayer", typeof(RectTransform)).GetComponent<RectTransform>();
            popupLayer.SetParent(rootObject.transform);

            uiRoot.Construct(panelLayer, popupLayer);
            return uiRoot;
        }
    }
}
