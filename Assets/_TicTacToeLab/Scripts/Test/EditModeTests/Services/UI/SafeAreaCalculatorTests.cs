using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class SafeAreaCalculatorTests
    {
        public sealed class InsetScenario
        {
            public readonly Rect SafeArea;
            public readonly float ScreenWidth;
            public readonly float ScreenHeight;
            public readonly Vector2 ExpectedMin;
            public readonly Vector2 ExpectedMax;

            public InsetScenario(Rect safeArea, float screenWidth, float screenHeight, Vector2 expectedMin, Vector2 expectedMax)
            {
                SafeArea = safeArea;
                ScreenWidth = screenWidth;
                ScreenHeight = screenHeight;
                ExpectedMin = expectedMin;
                ExpectedMax = expectedMax;
            }

            public override string ToString()
            {
                return $"{SafeArea.width}x{SafeArea.height} safe area on a {ScreenWidth}x{ScreenHeight} screen";
            }
        }

        private static IEnumerable<InsetScenario> RepresentativeInsets()
        {
            // A top cutout of 234px on a 1080x2340 screen insets only the top.
            yield return new InsetScenario(new Rect(0f, 0f, 1080f, 2106f), 1080f, 2340f, new Vector2(0f, 0f), new Vector2(1f, 0.9f));
            // A bottom gesture bar of 320px on a 1440x3200 screen insets only the bottom.
            yield return new InsetScenario(new Rect(0f, 320f, 1440f, 2880f), 1440f, 3200f, new Vector2(0f, 0.1f), new Vector2(1f, 1f));
            // 60px side, 120px bottom, and 180px top insets combine on a 1200x2400 screen.
            yield return new InsetScenario(new Rect(60f, 120f, 1080f, 2100f), 1200f, 2400f, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.925f));
        }

        // --- Full screen ---

        [Test]
        public void A_full_screen_safe_area_stretches_anchors_across_the_whole_screen()
        {
            SafeAreaInsets insets = SafeAreaCalculator.Calculate(new Rect(0f, 0f, 1080f, 2340f), 1080f, 2340f);

            Assert.That(insets.AnchorMin, Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(insets.AnchorMax, Is.EqualTo(new Vector2(1f, 1f)));
        }

        // --- Insets ---

        [TestCaseSource(nameof(RepresentativeInsets))]
        public void Insets_map_to_literal_normalized_anchors_across_representative_screens(InsetScenario scenario)
        {
            SafeAreaInsets insets = SafeAreaCalculator.Calculate(scenario.SafeArea, scenario.ScreenWidth, scenario.ScreenHeight);

            Assert.That(insets.AnchorMin, Is.EqualTo(scenario.ExpectedMin));
            Assert.That(insets.AnchorMax, Is.EqualTo(scenario.ExpectedMax));
        }

        // --- Invalid dimensions ---

        [TestCase(-1080f, 2340f)]
        [TestCase(1080f, -2340f)]
        [TestCase(0f, 2340f)]
        [TestCase(1080f, 0f)]
        public void Zero_or_negative_screen_dimensions_fall_back_to_full_stretch_anchors(float screenWidth, float screenHeight)
        {
            SafeAreaInsets insets = SafeAreaCalculator.Calculate(new Rect(0f, 100f, 1080f, 2000f), screenWidth, screenHeight);

            Assert.That(insets.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(insets.AnchorMax, Is.EqualTo(Vector2.one));
        }
    }
}
