#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GameplayPanelTests
    {
        private const string GAMEPLAY_PANEL_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/Panel/GameplayPanel.prefab";

        private GameObject _instance;
        private GameplayPanel _sut;
        private BoardPresenterHarness _boardPresenterHarness;

        [SetUp]
        public void CreateProductionGameplayPanel()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GAMEPLAY_PANEL_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, "The production gameplay panel prefab must exist at its shipped path.");

            _instance = Object.Instantiate(prefab);
            _sut = _instance.GetComponent<GameplayPanel>();
            Assert.That(_sut, Is.Not.Null, "The production gameplay panel prefab must carry its component.");

            _boardPresenterHarness = new BoardPresenterHarness();
        }

        [UnityTearDown]
        public IEnumerator DestroyGameplayPanel()
        {
            if (_instance == null)
            {
                yield break;
            }

            Object.Destroy(_instance);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _instance == null,
                "The gameplay panel fixture should be destroyed after the test.");
        }

        [Test]
        public void Gameplay_shows_the_presenters_current_turn_when_it_is_shown()
        {
            // Arrange
            // X owns the first turn; this places a mark so the presenter's current turn is not the default one.
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Act
            _sut.Setup(_boardPresenterHarness.Presenter, () => { });

            // Assert
            Assert.That(TurnText(), Is.EqualTo("O's turn"), "Gameplay should show the presenter's current turn.");
        }

        [Test]
        public void Gameplay_follows_the_presenters_turn_change_notifications()
        {
            // Arrange
            _sut.Setup(_boardPresenterHarness.Presenter, () => { });

            // Act
            _boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));

            // Assert
            Assert.That(TurnText(), Is.EqualTo("O's turn"), "Gameplay should follow the presenter's turn changes.");
        }

        private string TurnText()
        {
            return _sut.GetComponentInChildren<TMP_Text>(includeInactive: true).text;
        }
    }
}
#endif
