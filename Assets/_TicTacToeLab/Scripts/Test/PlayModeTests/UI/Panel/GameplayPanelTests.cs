#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GameplayPanelTests
    {
        private GeneratedApplicationUIFixture _fixture;
        private GameplayPanel _sut;
        private Button _backButton;
        private TMP_Text _turnText;
        private BoardPresenterHarness _boardPresenterHarness;

        [SetUp]
        public void CreateGeneratedGameplayPanel()
        {
            _fixture = new GeneratedApplicationUIFixture();
            (_sut, _backButton, _turnText) = _fixture.CreateGameplayPanel();
            _boardPresenterHarness = new BoardPresenterHarness();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedGameplayPanel()
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
            _sut.Setup(_boardPresenterHarness.Presenter, () => { });

            // Assert
            Assert.That(_turnText.text, Is.EqualTo("O's turn"), "Gameplay should show the presenter's current turn.");
        }

        [Test]
        public void Gameplay_follows_the_presenters_turn_change_notifications()
        {
            // Arrange
            _sut.Setup(_boardPresenterHarness.Presenter, () => { });

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
            _sut.Setup(_boardPresenterHarness.Presenter, () => backCount++);

            // Act
            _backButton.onClick.Invoke();

            // Assert
            Assert.That(backCount, Is.EqualTo(1), "The Back button should route to the supplied back action exactly once.");
        }
    }
}
#endif
