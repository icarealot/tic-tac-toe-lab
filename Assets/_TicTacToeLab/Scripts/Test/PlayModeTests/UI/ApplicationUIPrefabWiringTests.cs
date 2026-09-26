#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class ApplicationUIPrefabWiringTests
    {
        private const string APPLICATION_UI_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/ApplicationUI.prefab";

        private readonly List<GameObject> _productionInstances = new();

        [UnityTearDown]
        public IEnumerator DestroyProductionInstances()
        {
            GameObject[] instances = _productionInstances.ToArray();
            _productionInstances.Clear();

            foreach (GameObject instance in instances)
            {
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => instances.All(instance => instance == null),
                "Every instantiated production object should be destroyed after the test.");
        }

        [Test]
        public void Showing_the_production_main_menu_routes_its_start_button_to_the_supplied_action()
        {
            // Arrange
            ApplicationUI sut = CreateProductionApplicationUI();
            int startCount = 0;
            sut.ShowMainMenu(() => startCount++);
            MainMenuPanel mainMenu = RequireSingleWindow<MainMenuPanel>(sut);
            Button startButton = RequireSingleButton(mainMenu);
            Assert.That(startButton.isActiveAndEnabled, Is.True, "The production main menu's Start button should be active and enabled.");
            Assert.That(startButton.interactable, Is.True, "The production main menu's Start button should be interactable.");

            // Act
            startButton.onClick.Invoke();

            // Assert
            Assert.That(mainMenu.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production main menu.");
            Assert.That(startCount, Is.EqualTo(1), "The production main menu's Start button should invoke the supplied action exactly once.");
        }

        [Test]
        public void Showing_production_gameplay_presents_the_current_turn_and_routes_its_back_button()
        {
            // Arrange
            ApplicationUI sut = CreateProductionApplicationUI();
            BoardPresenterHarness boardPresenterHarness = new();
            // X owns the first turn; this places a mark so the presenter's current turn is not the default one.
            boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));
            int backCount = 0;
            sut.ShowGameplay(boardPresenterHarness.Presenter, () => backCount++);
            GameplayPanel gameplay = RequireSingleWindow<GameplayPanel>(sut);
            Button backButton = RequireSingleButton(gameplay);
            Assert.That(backButton.isActiveAndEnabled, Is.True, "The production gameplay's Back button should be active and enabled.");
            Assert.That(backButton.interactable, Is.True, "The production gameplay's Back button should be interactable.");

            // Act
            backButton.onClick.Invoke();

            // Assert
            Assert.That(gameplay.gameObject.activeInHierarchy, Is.True, "The production application UI should show production gameplay.");
            Assert.That(
                VisibleTexts(gameplay),
                Has.Exactly(1).EqualTo("O's turn"),
                "Production gameplay should present the presenter's current turn.");
            Assert.That(backCount, Is.EqualTo(1), "The production gameplay's Back button should invoke the supplied action exactly once.");
        }

        [Test]
        public void Showing_the_production_quit_confirmation_routes_both_buttons_to_the_supplied_actions_exactly_once()
        {
            // Arrange
            ApplicationUI sut = CreateProductionApplicationUI();
            int quitCount = 0;
            int cancelCount = 0;
            sut.ShowQuitConfirmation(() => quitCount++, () => cancelCount++);
            ConfirmQuitPopup quitConfirmation = RequireSingleWindow<ConfirmQuitPopup>(sut);
            Button[] buttons = RequireButtons(quitConfirmation, 2);
            foreach (Button button in buttons)
            {
                Assert.That(button.isActiveAndEnabled, Is.True, "Every production quit confirmation button should be active and enabled.");
                Assert.That(button.interactable, Is.True, "Every production quit confirmation button should be interactable.");
            }

            // Act
            int[] suppliedCallbacksAfterEachButton = new int[buttons.Length];
            for (int index = 0; index < buttons.Length; index++)
            {
                buttons[index].onClick.Invoke();
                suppliedCallbacksAfterEachButton[index] = quitCount + cancelCount;
            }

            // Assert
            Assert.That(quitConfirmation.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production quit confirmation.");
            for (int index = 0; index < suppliedCallbacksAfterEachButton.Length; index++)
            {
                int callbacksBefore = index == 0 ? 0 : suppliedCallbacksAfterEachButton[index - 1];
                Assert.That(
                    suppliedCallbacksAfterEachButton[index],
                    Is.EqualTo(callbacksBefore + 1),
                    "Activating each production quit confirmation button should invoke exactly one supplied action.");
            }

            Assert.That(quitCount, Is.EqualTo(1), "Activating both production quit confirmation buttons should invoke the supplied quit action exactly once.");
            Assert.That(cancelCount, Is.EqualTo(1), "Activating both production quit confirmation buttons should invoke the supplied cancel action exactly once.");
        }

        [TestCase(Outcome.XWin, "X Wins!")]
        [TestCase(Outcome.OWin, "O Wins!")]
        [TestCase(Outcome.Draw, "Draw!")]
        public void Showing_each_terminal_outcome_presents_it_and_routes_the_production_continue_button(Outcome outcome, string expectedTitle)
        {
            // Arrange
            ApplicationUI sut = CreateProductionApplicationUI();
            int continueCount = 0;
            sut.ShowOutcome(outcome, () => continueCount++);
            OutcomePopup outcomePopup = RequireSingleWindow<OutcomePopup>(sut);
            Button continueButton = RequireSingleButton(outcomePopup);
            Assert.That(continueButton.isActiveAndEnabled, Is.True, "The production outcome popup's Continue button should be active and enabled.");
            Assert.That(continueButton.interactable, Is.True, "The production outcome popup's Continue button should be interactable.");

            // Act
            continueButton.onClick.Invoke();

            // Assert
            Assert.That(outcomePopup.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production outcome popup.");
            Assert.That(
                VisibleTexts(outcomePopup),
                Has.Exactly(1).EqualTo(expectedTitle),
                "The production outcome popup should present the terminal outcome it was shown.");
            Assert.That(continueCount, Is.EqualTo(1), "The production outcome popup's Continue button should invoke the supplied action exactly once.");
        }

        private ApplicationUI CreateProductionApplicationUI()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(APPLICATION_UI_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, $"The production prefab must exist at {APPLICATION_UI_PREFAB_PATH}.");

            GameObject instance = Object.Instantiate(prefab);
            _productionInstances.Add(instance);

            ApplicationUI applicationUI = instance.GetComponent<ApplicationUI>();
            Assert.That(applicationUI, Is.Not.Null, "The production application UI prefab must carry ApplicationUI.");
            return applicationUI;
        }

        private static T RequireSingleWindow<T>(ApplicationUI applicationUI) where T : Component
        {
            T[] windows = applicationUI.GetComponentsInChildren<T>(true);
            Assert.That(windows, Has.Length.EqualTo(1), $"The production application UI should show exactly one {typeof(T).Name} window.");
            return windows[0];
        }

        private static Button RequireSingleButton(Component window)
        {
            return RequireButtons(window, 1)[0];
        }

        private static Button[] RequireButtons(Component window, int expectedCount)
        {
            Button[] buttons = window.GetComponentsInChildren<Button>(true);
            Assert.That(buttons, Has.Length.EqualTo(expectedCount), $"The production {window.GetType().Name} window should provide exactly {expectedCount} buttons.");
            return buttons;
        }

        private static string[] VisibleTexts(Component window)
        {
            return window.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text.isActiveAndEnabled)
                .Select(text => text.text)
                .ToArray();
        }
    }
}
#endif
