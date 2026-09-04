using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public readonly struct SafeAreaInsets
    {
        public Vector2 AnchorMin { get; }
        public Vector2 AnchorMax { get; }

        public SafeAreaInsets(Vector2 anchorMin, Vector2 anchorMax)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
        }
    }
}
