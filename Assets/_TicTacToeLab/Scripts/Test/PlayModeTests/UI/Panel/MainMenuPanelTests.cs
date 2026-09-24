#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class MainMenuPanelTests
    {
        private GeneratedApplicationUIFixture _fixture;
        private MainMenuPanel _sut;
        private Button _startButton;

        [SetUp]
        public void CreateGeneratedMainMenuPanel()
        {
            _fixture = new GeneratedApplicationUIFixture();
            (_sut, _startButton) = _fixture.CreateMainMenuPanel();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedMainMenuPanel()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Starting_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int startCount = 0;
            _sut.Setup(() => startCount++);

            // Act
            _startButton.onClick.Invoke();

            // Assert
            Assert.That(startCount, Is.EqualTo(1), "The Start button should route to the supplied start action exactly once.");
        }
    }
}
#endif
