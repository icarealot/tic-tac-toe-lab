#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class OutcomeScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private OutcomeScreen _sut;
        private TMP_Text _titleText;

        [SetUp]
        public void CreateGeneratedOutcomeScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _, _titleText) = _fixture.CreateOutcomeScreen();
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
            _sut.Setup(outcome, new GameSetup(GameMode.Pvp, null), () => { });

            // Assert
            Assert.That(_titleText.text, Is.EqualTo(expectedTitle), "The screen should report the outcome it was shown.");
        }

        [TestCase(Outcome.XWin, "You Win!")]
        [TestCase(Outcome.OWin, "Bot Wins!")]
        [TestCase(Outcome.Draw, "Draw!")]
        public void A_PvE_setup_uses_the_mode_specific_terminal_title(Outcome outcome, string expectedTitle)
        {
            // Act
            _sut.Setup(outcome, new GameSetup(GameMode.Pve, BotDifficulty.Professional), () => { });

            // Assert
            Assert.That(_titleText.text, Is.EqualTo(expectedTitle));
        }
    }
}
#endif
