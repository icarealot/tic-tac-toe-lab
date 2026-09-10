using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IUIRoot
    {
        public RectTransform PanelLayer { get; }
        public RectTransform PopupLayer { get; }
    }
}
