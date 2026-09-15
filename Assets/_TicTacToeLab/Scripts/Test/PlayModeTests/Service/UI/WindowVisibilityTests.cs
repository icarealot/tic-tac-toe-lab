#if UNITY_EDITOR
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class WindowVisibilityTests
    {
        private sealed class TestWindow : Window
        {
        }

        private GameObject _host;
        private Window _sut;

        [SetUp]
        public void CreateIsolatedWindow()
        {
            _host = new GameObject("WindowVisibilityTests");
            _sut = _host.AddComponent<TestWindow>();
        }

        [TearDown]
        public void DestroyIsolatedWindow()
        {
            if (_host != null)
            {
                Object.Destroy(_host);
            }
        }

        [Test]
        public void A_hidden_window_is_invisible_non_interactive_and_stops_blocking_raycasts()
        {
            _sut.Hide();

            CanvasGroup canvasGroup = _sut.GetComponent<CanvasGroup>();
            Assert.That(_sut.IsVisible, Is.False);
            Assert.That(canvasGroup.alpha, Is.Zero);
            Assert.That(canvasGroup.interactable, Is.False);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);
        }

        [Test]
        public void A_window_shown_after_hiding_is_visible_interactive_and_blocks_raycasts()
        {
            _sut.Hide();
            _sut.Show();

            CanvasGroup canvasGroup = _sut.GetComponent<CanvasGroup>();
            Assert.That(_sut.IsVisible, Is.True);
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);
        }
    }
}
#endif
