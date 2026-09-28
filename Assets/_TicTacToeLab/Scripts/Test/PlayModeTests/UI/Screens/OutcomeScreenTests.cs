#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class OutcomeScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private OutcomeScreen _sut;
        private Button _continueButton;
        private TMP_Text _titleText;

        [SetUp]
        public void CreateGeneratedOutcomeScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _continueButton, _titleText) = _fixture.CreateOutcomeScreen();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedOutcomeScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [TestCase(Outcome.XWin, "X Wins!")]
        [TestCase(Outcome.OWin, "O Wins!")]
        [TestCase(Outcome.Draw, "Draw!")]
        public void The_screen_reports_the_terminal_outcome_as_its_title(Outcome outcome, string expectedTitle)
        {
            // Act
            _sut.Setup(outcome, () => { });

            // Assert
            Assert.That(_titleText.text, Is.EqualTo(expectedTitle), "The screen should report the outcome it was shown.");
        }

        [Test]
        public void Continuing_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int continueCount = 0;
            _sut.Setup(Outcome.XWin, () => continueCount++);

            // Act
            _continueButton.onClick.Invoke();

            // Assert
            Assert.That(continueCount, Is.EqualTo(1), "The Continue button should route to the supplied continue action exactly once.");
        }
    }
}
#endif
