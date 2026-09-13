#if UNITY_EDITOR
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Window visibility state on one isolated object: Hide and Show drive the window's CanvasGroup
    /// between its hidden and shown states, and IsVisible reports which state the window is in.
    /// Alpha, interaction, and raycast blocking are the contract; styling and the production
    /// windows' content and layout are not.
    /// </summary>
    public sealed class WindowVisibilityTests
    {
        /// <summary>The production Window is abstract; this subclass carries no extra behavior.</summary>
        private sealed class TestWindow : Window { }

        private GameObject _host;
        private Window _window;

        [SetUp]
        public void CreateIsolatedWindow()
        {
            _host = new GameObject("WindowVisibilityTests");
            _window = _host.AddComponent<TestWindow>();
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
            _window.Hide();

            CanvasGroup canvasGroup = _window.GetComponent<CanvasGroup>();
            Assert.That(_window.IsVisible, Is.False);
            Assert.That(canvasGroup.alpha, Is.Zero);
            Assert.That(canvasGroup.interactable, Is.False);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);
        }

        [Test]
        public void A_window_shown_after_hiding_is_visible_interactive_and_blocks_raycasts()
        {
            _window.Hide();
            _window.Show();

            CanvasGroup canvasGroup = _window.GetComponent<CanvasGroup>();
            Assert.That(_window.IsVisible, Is.True);
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);
        }
    }
}
#endif
