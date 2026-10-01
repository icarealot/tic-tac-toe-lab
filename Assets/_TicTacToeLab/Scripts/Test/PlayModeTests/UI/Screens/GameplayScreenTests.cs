#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GameplayScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private GameplayScreen _sut;
        private TMP_Text _turnText;
        private BoardPresenterHarness _boardPresenterHarness;

        [SetUp]
        public void CreateGeneratedGameplayScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _, _turnText) = _fixture.CreateGameplayScreen();
            _boardPresenterHarness = new BoardPresenterHarness();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedGameplayScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void PvP_gameplay_shows_the_presenters_current_turn_when_it_is_set_up()
        {
            // Arrange
            // X owns the first turn by default.

            // Act
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => { });

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("X's turn"), "Gameplay should show the presenter's current turn.");
        }

        [Test]
        public void PvE_gameplay_reports_the_bot_turn_after_the_human_places_a_mark()
        {
            // Arrange
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pve, BotDifficulty.Amateur), () => { });

            // Act
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("Bot is thinking… (O)"));
        }

        [Test]
        public void PvE_gameplay_reports_the_human_turn_after_the_bot_places_a_mark()
        {
            // Arrange
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pve, BotDifficulty.Amateur), () => { });

            // Act
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 1));

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("Your turn (X)"));
        }

        [Test]
        public void PvP_gameplay_reports_the_O_turn_after_X_places_a_mark()
        {
            // Arrange
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => { });

            // Act
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("O's turn"), "Gameplay should follow the presenter's turn changes.");
        }

        [UnityTest]
        public IEnumerator Gameplay_detaches_from_the_presenter_when_destroyed()
        {
            // Arrange
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => { });
            string turnBeforeDestruction = _turnText.text;
            Object.Destroy(_sut);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _sut == null,
                "The gameplay screen should be destroyed before its detachment is verified.");

            // Act
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Assert
            Assert.That(
                _turnText.text,
                Is.EqualTo(turnBeforeDestruction),
                "Destroying gameplay should detach it from the board presenter's turn changes.");
        }
    }
}
#endif
