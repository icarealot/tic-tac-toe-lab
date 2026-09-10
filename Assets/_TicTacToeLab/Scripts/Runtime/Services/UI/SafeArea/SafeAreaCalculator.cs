using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public static class SafeAreaCalculator
    {
        public static SafeAreaInsets Calculate(Rect safeArea, float screenWidth, float screenHeight)
        {
            if (screenWidth <= 0f || screenHeight <= 0f)
            {
                return new SafeAreaInsets(Vector2.zero, Vector2.one);
            }

            Vector2 anchorMin = new(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
            Vector2 anchorMax = new(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);

            return new SafeAreaInsets(anchorMin, anchorMax);
        }
    }
}
