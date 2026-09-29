#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class HomeScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private HomeScreen _sut;
        private Button _pvpButton;
        private Button _pveButton;

        [SetUp]
        public void CreateGeneratedHomeScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _pvpButton, _pveButton) = _fixture.CreateHomeScreen();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedHomeScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Selecting_PvP_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int pvpCount = 0;
            int pveCount = 0;
            _sut.Setup(() => pvpCount++, () => pveCount++);

            // Act
            _pvpButton.onClick.Invoke();

            // Assert
            Assert.That(pvpCount, Is.EqualTo(1), "The PvP button should invoke its supplied action exactly once.");
            Assert.That(pveCount, Is.EqualTo(0), "Selecting PvP should not invoke the PvE action.");
        }

        [Test]
        public void Selecting_PvE_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int pvpCount = 0;
            int pveCount = 0;
            _sut.Setup(() => pvpCount++, () => pveCount++);

            // Act
            _pveButton.onClick.Invoke();

            // Assert
            Assert.That(pveCount, Is.EqualTo(1), "The PvE button should invoke its supplied action exactly once.");
            Assert.That(pvpCount, Is.EqualTo(0), "Selecting PvE should not invoke the PvP action.");
        }

        [UnityTest]
        public IEnumerator Destroying_the_screen_detaches_every_button_listener()
        {
            // Arrange
            int pvpCount = 0;
            int pveCount = 0;
            _sut.Setup(() => pvpCount++, () => pveCount++);
            Object.Destroy(_sut);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _sut == null,
                "The Home screen should be destroyed before its listeners are verified.");

            // Act
            _pvpButton.onClick.Invoke();
            _pveButton.onClick.Invoke();

            // Assert
            Assert.That(pvpCount, Is.EqualTo(0), "Destroying Home should detach the PvP button listener.");
            Assert.That(pveCount, Is.EqualTo(0), "Destroying Home should detach the PvE button listener.");
        }
    }
}
#endif
