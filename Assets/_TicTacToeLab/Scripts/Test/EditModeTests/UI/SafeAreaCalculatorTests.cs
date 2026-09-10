using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class SafeAreaCalculatorTests
    {
        private float _screenWidth;
        private float _screenHeight;

        [SetUp]
        public void SetUp()
        {
            _screenWidth = 1080f;
            _screenHeight = 2340f;
        }

        [Test]
        public void A_full_screen_safe_area_produces_a_full_stretch_rect_with_no_inset()
        {
            Rect safeArea = new(0f, 0f, _screenWidth, _screenHeight);

            SafeAreaInsets insets = SafeAreaCalculator.Calculate(safeArea, _screenWidth, _screenHeight);

            Assert.That(insets.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(insets.AnchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void A_top_cutout_insets_only_the_top()
        {
            Rect safeArea = new(0f, 0f, _screenWidth, 2200f);

            SafeAreaInsets insets = SafeAreaCalculator.Calculate(safeArea, _screenWidth, _screenHeight);

            Assert.That(insets.AnchorMin.y, Is.EqualTo(0f));
            Assert.That(insets.AnchorMax.y, Is.LessThan(1f));
        }

        [Test]
        public void A_bottom_gesture_bar_insets_only_the_bottom()
        {
            Rect safeArea = new(0f, 140f, _screenWidth, 2200f);

            SafeAreaInsets insets = SafeAreaCalculator.Calculate(safeArea, _screenWidth, _screenHeight);

            Assert.That(insets.AnchorMin.y, Is.GreaterThan(0f));
            Assert.That(insets.AnchorMax.y, Is.EqualTo(1f));
        }

        [TestCase(1080f, 2340f)]
        [TestCase(1440f, 3200f)]
        [TestCase(750f, 1334f)]
        public void Insets_are_correct_across_different_screen_sizes(float screenWidth, float screenHeight)
        {
            Rect safeArea = new(0f, 100f, screenWidth, screenHeight - 150f);

            SafeAreaInsets insets = SafeAreaCalculator.Calculate(safeArea, screenWidth, screenHeight);

            Assert.That(insets.AnchorMin, Is.EqualTo(new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight)));
            Assert.That(insets.AnchorMax, Is.EqualTo(new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight)));
        }
    }
}
