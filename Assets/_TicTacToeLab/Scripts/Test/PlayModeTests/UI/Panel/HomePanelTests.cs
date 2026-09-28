#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class HomePanelTests
    {
        private GeneratedAppUIFixture _fixture;
        private HomePanel _sut;
        private Button _startButton;

        [SetUp]
        public void CreateGeneratedHomePanel()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _startButton) = _fixture.CreateHomePanel();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedHomePanel()
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
