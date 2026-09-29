#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GameplayScreenTests
    {
        private GeneratedAppUIFixture _fixture;
        private GameplayScreen _sut;
        private Button _backButton;
        private TMP_Text _turnText;
        private BoardPresenterHarness _boardPresenterHarness;

        [SetUp]
        public void CreateGeneratedGameplayScreen()
        {
            _fixture = new GeneratedAppUIFixture();
            (_sut, _backButton, _turnText) = _fixture.CreateGameplayScreen();
            _boardPresenterHarness = new BoardPresenterHarness();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedGameplayScreen()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Gameplay_shows_the_presenters_current_turn_when_it_is_set_up()
        {
            // Arrange
            // X owns the first turn; this places a mark so the presenter's current turn is not the default one.
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Act
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => { });

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("O's turn"), "Gameplay should show the presenter's current turn.");
        }

        [Test]
        public void Gameplay_follows_the_presenters_turn_change_notifications()
        {
            // Arrange
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => { });

            // Act
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("O's turn"), "Gameplay should follow the presenter's turn changes.");
        }

        [Test]
        public void Gameplay_back_routes_exactly_once_to_the_supplied_action()
        {
            // Arrange
            int backCount = 0;
            _sut.Setup(_boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => backCount++);

            // Act
            _backButton.onClick.Invoke();

            // Assert
            Assert.That(backCount, Is.EqualTo(1), "The Back button should route to the supplied back action exactly once.");
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
