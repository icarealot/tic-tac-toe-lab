#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ApplicationUITests
    {
        private const string APPLICATION_UI_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/ApplicationUI.prefab";

        private GameObject _instance;
        private ApplicationUI _sut;
        private BoardPresenter _boardPresenter;

        [SetUp]
        public void CreateProductionApplicationUI()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(APPLICATION_UI_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, "The production application UI prefab must exist at its shipped path.");

            _instance = Object.Instantiate(prefab);
            _sut = _instance.GetComponent<ApplicationUI>();
            Assert.That(_sut, Is.Not.Null, "The production application UI prefab must carry its adapter component.");

            _boardPresenter = new BoardPresenterHarness().Presenter;
        }

        [UnityTearDown]
        public IEnumerator DestroyApplicationUI()
        {
            if (_instance != null)
            {
                Object.Destroy(_instance);
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _instance == null,
                "The application UI fixture should be destroyed after the test.");
        }

        [Test]
        public void Showing_the_main_menu_creates_exactly_one_panel_and_no_popup()
        {
            // Act
            _sut.ShowMainMenu(() => { });

            // Assert
            Assert.That(MainMenus(), Has.Length.EqualTo(1), "The adapter should own exactly one current panel.");
            Assert.That(QuitConfirmations(), Is.Empty, "Showing a panel should not create a popup.");
            Assert.That(Outcomes(), Is.Empty, "Showing a panel should not create a popup.");
        }

        [Test]
        public void Starting_from_the_main_menu_routes_to_the_supplied_action()
        {
            // Arrange
            bool started = false;
            _sut.ShowMainMenu(() => started = true);

            // Act
            TestSerializedReference.ReadButton(CurrentMainMenu(), "_startButton").onClick.Invoke();

            // Assert
            Assert.That(started, Is.True, "The production Start button should route to the supplied start action.");
        }

        [UnityTest]
        public IEnumerator Showing_gameplay_replaces_and_destroys_the_previous_panel()
        {
            // Arrange
            _sut.ShowMainMenu(() => { });
            MainMenuPanel mainMenu = CurrentMainMenu();

            // Act
            _sut.ShowGameplay(_boardPresenter, () => { });

            // Assert
            Assert.That(Gameplays(), Has.Length.EqualTo(1), "The adapter should own exactly one current panel.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => mainMenu == null,
                "Replacing the panel should destroy the previous panel instance.");
        }

        [Test]
        public void Gameplay_back_routes_to_the_supplied_action()
        {
            // Arrange
            bool wentBack = false;
            _sut.ShowGameplay(_boardPresenter, () => wentBack = true);

            // Act
            TestSerializedReference.ReadButton(CurrentGameplay(), "_backButton").onClick.Invoke();

            // Assert
            Assert.That(wentBack, Is.True, "The production Back button should route to the supplied back action.");
        }

        [Test]
        public void Showing_quit_confirmation_presents_one_popup_over_the_still_present_gameplay()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            GameplayPanel gameplay = CurrentGameplay();

            // Act
            _sut.ShowQuitConfirmation(() => { }, () => { });

            // Assert
            Assert.That(QuitConfirmations(), Has.Length.EqualTo(1), "The adapter should own exactly one current popup.");
            Assert.That(Outcomes(), Is.Empty, "The confirmation should be the only current popup.");
            Assert.That(CurrentGameplay(), Is.SameAs(gameplay), "Gameplay should stay present beneath the confirmation.");
        }

        [UnityTest]
        public IEnumerator Closing_quit_confirmation_destroys_the_popup_and_preserves_gameplay()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            GameplayPanel gameplay = CurrentGameplay();
            _sut.ShowQuitConfirmation(() => { }, () => { });
            ConfirmQuitPopup confirmation = CurrentConfirmation();

            // Act
            _sut.CloseQuitConfirmation();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => confirmation == null,
                "Closing quit confirmation should destroy its popup instance.");
            Assert.That(QuitConfirmations(), Is.Empty);
            Assert.That(CurrentGameplay(), Is.SameAs(gameplay), "Gameplay should stay present after the confirmation closes.");
        }

        [Test]
        public void Confirming_quit_routes_to_the_supplied_action()
        {
            // Arrange
            bool quit = false;
            _sut.ShowQuitConfirmation(() => quit = true, () => { });

            // Act
            TestSerializedReference.ReadButton(CurrentConfirmation(), "_yesButton").onClick.Invoke();

            // Assert
            Assert.That(quit, Is.True, "The production Yes button should route to the supplied quit action.");
        }

        [Test]
        public void Answering_no_routes_to_the_supplied_cancel_action()
        {
            // Arrange
            bool cancelled = false;
            _sut.ShowQuitConfirmation(() => { }, () => cancelled = true);

            // Act
            TestSerializedReference.ReadButton(CurrentConfirmation(), "_noButton").onClick.Invoke();

            // Assert
            Assert.That(cancelled, Is.True, "The production No button should route to the supplied cancel action.");
        }

        [UnityTest]
        public IEnumerator Showing_the_outcome_replaces_the_previous_popup_and_preserves_gameplay()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            GameplayPanel gameplay = CurrentGameplay();
            _sut.ShowQuitConfirmation(() => { }, () => { });
            ConfirmQuitPopup confirmation = CurrentConfirmation();

            // Act
            _sut.ShowOutcome(Outcome.XWin, () => { });

            // Assert
            Assert.That(Outcomes(), Has.Length.EqualTo(1), "The adapter should own exactly one current popup.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => confirmation == null,
                "Replacing the popup should destroy the previous popup instance.");
            Assert.That(QuitConfirmations(), Is.Empty);
            Assert.That(CurrentGameplay(), Is.SameAs(gameplay), "Gameplay should stay present beneath the outcome.");
        }

        [UnityTest]
        public IEnumerator Closing_the_popup_destroys_the_outcome_popup()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            _sut.ShowOutcome(Outcome.Draw, () => { });
            OutcomePopup outcome = CurrentOutcome();

            // Act
            _sut.ClosePopup();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => outcome == null,
                "Closing the popup should destroy the outcome instance.");
            Assert.That(Outcomes(), Is.Empty);
        }

        [Test]
        public void Continuing_from_the_outcome_routes_to_the_supplied_action()
        {
            // Arrange
            bool continued = false;
            _sut.ShowOutcome(Outcome.OWin, () => continued = true);

            // Act
            TestSerializedReference.ReadButton(CurrentOutcome(), "_continueButton").onClick.Invoke();

            // Assert
            Assert.That(continued, Is.True, "The production Continue button should route to the supplied continue action.");
        }

        private MainMenuPanel CurrentMainMenu()
        {
            return _sut.GetComponentInChildren<MainMenuPanel>(includeInactive: true);
        }

        private GameplayPanel CurrentGameplay()
        {
            return _sut.GetComponentInChildren<GameplayPanel>(includeInactive: true);
        }

        private MainMenuPanel[] MainMenus()
        {
            return _sut.GetComponentsInChildren<MainMenuPanel>(includeInactive: true);
        }

        private GameplayPanel[] Gameplays()
        {
            return _sut.GetComponentsInChildren<GameplayPanel>(includeInactive: true);
        }

        private ConfirmQuitPopup CurrentConfirmation()
        {
            return _sut.GetComponentInChildren<ConfirmQuitPopup>(includeInactive: true);
        }

        private OutcomePopup CurrentOutcome()
        {
            return _sut.GetComponentInChildren<OutcomePopup>(includeInactive: true);
        }

        private ConfirmQuitPopup[] QuitConfirmations()
        {
            return _sut.GetComponentsInChildren<ConfirmQuitPopup>(includeInactive: true);
        }

        private OutcomePopup[] Outcomes()
        {
            return _sut.GetComponentsInChildren<OutcomePopup>(includeInactive: true);
        }
    }
}
#endif
