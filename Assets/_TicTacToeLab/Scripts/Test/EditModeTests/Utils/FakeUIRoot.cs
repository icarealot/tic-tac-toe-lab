using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    /// <summary>
    /// Plain EditMode substitute for the UI root. The window layers are real RectTransforms in the
    /// production scene, which EditMode deliberately does not hold, so both report null; parenting
    /// itself is verified against production wiring in PlayMode.
    /// </summary>
    public sealed class FakeUIRoot : IUIRoot
    {
        public RectTransform PanelLayer => null;
        public RectTransform PopupLayer => null;
    }
}
