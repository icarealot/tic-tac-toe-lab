#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BotSelectionScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private BotSelectionScreen _sut;
        private Button _amateurButton;
        private Button _professionalButton;
        private Button _backButton;

        [SetUp]
        public void CreateGeneratedBotSelectionScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _amateurButton, _professionalButton, _backButton) = _fixture.CreateBotSelectionScreen();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedBotSelectionScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Bot_selection_shows_only_named_choices_without_descriptions()
        {
            // Act
            string[] visibleTexts = _sut.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text.isActiveAndEnabled)
                .Select(text => text.text)
                .ToArray();

            // Assert
            Assert.That(
                visibleTexts,
                Is.EquivalentTo(new[] { "Amateur", "Professional", "Back" }),
                "Bot selection should show only the two named difficulty choices and Back.");
        }

        [Test]
        public void Selecting_Amateur_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            _sut.Setup(() => amateurCount++, () => professionalCount++, () => backCount++);

            // Act
            _amateurButton.onClick.Invoke();

            // Assert
            Assert.That(amateurCount, Is.EqualTo(1), "The Amateur button should invoke its supplied action exactly once.");
            Assert.That(professionalCount, Is.EqualTo(0), "Selecting Amateur should not invoke Professional.");
            Assert.That(backCount, Is.EqualTo(0), "Selecting Amateur should not invoke Back.");
        }

        [Test]
        public void Selecting_Professional_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            _sut.Setup(() => amateurCount++, () => professionalCount++, () => backCount++);

            // Act
            _professionalButton.onClick.Invoke();

            // Assert
            Assert.That(professionalCount, Is.EqualTo(1), "The Professional button should invoke its supplied action exactly once.");
            Assert.That(amateurCount, Is.EqualTo(0), "Selecting Professional should not invoke Amateur.");
            Assert.That(backCount, Is.EqualTo(0), "Selecting Professional should not invoke Back.");
        }

        [Test]
        public void Back_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            _sut.Setup(() => amateurCount++, () => professionalCount++, () => backCount++);

            // Act
            _backButton.onClick.Invoke();

            // Assert
            Assert.That(backCount, Is.EqualTo(1), "The Back button should invoke its supplied action exactly once.");
            Assert.That(amateurCount, Is.EqualTo(0), "Selecting Back should not invoke Amateur.");
            Assert.That(professionalCount, Is.EqualTo(0), "Selecting Back should not invoke Professional.");
        }

        [UnityTest]
        public IEnumerator Destroying_the_screen_detaches_every_button_listener()
        {
            // Arrange
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            _sut.Setup(() => amateurCount++, () => professionalCount++, () => backCount++);

            // Act
            Object.Destroy(_sut);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _sut == null,
                "The bot-selection screen should be destroyed before listener detachment is verified.");
            _amateurButton.onClick.Invoke();
            _professionalButton.onClick.Invoke();
            _backButton.onClick.Invoke();

            // Assert
            Assert.That(amateurCount, Is.EqualTo(0), "Destroying the screen should detach the Amateur listener.");
            Assert.That(professionalCount, Is.EqualTo(0), "Destroying the screen should detach the Professional listener.");
            Assert.That(backCount, Is.EqualTo(0), "Destroying the screen should detach the Back listener.");
        }
    }
}
#endif
