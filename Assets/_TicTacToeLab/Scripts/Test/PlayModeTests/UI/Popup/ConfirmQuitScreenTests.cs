#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ConfirmQuitScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private ConfirmQuitScreen _sut;
        private Button _yesButton;
        private Button _noButton;

        [SetUp]
        public void CreateGeneratedConfirmQuitScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _yesButton, _noButton) = _fixture.CreateConfirmQuitScreen();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedConfirmQuitScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Confirming_routes_exactly_once_to_the_supplied_quit_action()
        {
            // Arrange
            int quitCount = 0;
            int cancelCount = 0;
            _sut.Setup(() => quitCount++, () => cancelCount++);

            // Act
            _yesButton.onClick.Invoke();

            // Assert
            Assert.That(quitCount, Is.EqualTo(1), "The Yes button should route to the supplied quit action exactly once.");
            Assert.That(cancelCount, Is.EqualTo(0), "The Yes button should not route to the cancel action.");
        }

        [Test]
        public void Answering_no_routes_exactly_once_to_the_supplied_cancel_action()
        {
            // Arrange
            int quitCount = 0;
            int cancelCount = 0;
            _sut.Setup(() => quitCount++, () => cancelCount++);

            // Act
            _noButton.onClick.Invoke();

            // Assert
            Assert.That(cancelCount, Is.EqualTo(1), "The No button should route to the supplied cancel action exactly once.");
            Assert.That(quitCount, Is.EqualTo(0), "The No button should not route to the quit action.");
        }
    }
}
#endif
