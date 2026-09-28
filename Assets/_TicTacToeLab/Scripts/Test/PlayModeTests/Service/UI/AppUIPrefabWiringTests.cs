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
    public sealed class AppUIPrefabWiringTests
    {
        private const string APP_UI_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/UI/AppUI.prefab";

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
        public void Showing_the_production_home_role_routes_its_start_button_to_the_supplied_action()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int startCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => startCount++));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            Button startButton = RequireButton(home, "Start");
            Assert.That(startButton.isActiveAndEnabled, Is.True, "The production Home screen's Start button should be active and enabled.");
            Assert.That(startButton.interactable, Is.True, "The production Home screen's Start button should be interactable.");

            // Act
            startButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production Home screen.");
            Assert.That(startCount, Is.EqualTo(1), "The production Home screen's Start button should invoke the supplied action exactly once.");
        }

        [Test]
        public void Showing_the_production_gameplay_role_replaces_the_base_screen_presents_the_current_turn_and_routes_its_back_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            BoardPresenterHarness boardPresenterHarness = new();
            // X owns the first turn; this places a mark so the presenter's current turn is not the default one.
            boardPresenterHarness.RaisePress(new CellCoordinate(0, 0));
            int backCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IGameplayScreen>(screen => screen.Setup(boardPresenterHarness.Presenter, () => backCount++));
            GameplayScreen gameplay = RequireSingleScreen<GameplayScreen>(sut);
            Button backButton = RequireButton(gameplay, "Back");
            Assert.That(backButton.isActiveAndEnabled, Is.True, "The production gameplay's Back button should be active and enabled.");
            Assert.That(backButton.interactable, Is.True, "The production gameplay's Back button should be interactable.");

            // Act
            backButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.False, "The production Gameplay registration should replace the active base screen.");
            Assert.That(gameplay.gameObject.activeInHierarchy, Is.True, "The production application UI should show production gameplay.");
            Assert.That(
                VisibleTexts(gameplay),
                Is.EquivalentTo(new[] { "Back", "O's turn" }),
                "Production gameplay should present the current turn and its Back control as its complete visible state.");
            Assert.That(backCount, Is.EqualTo(1), "The production gameplay's Back button should invoke the supplied action exactly once.");
        }

        [Test]
        public void Showing_the_production_confirm_quit_role_over_a_base_preserves_the_base_and_routes_both_choices_exactly_once()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int quitCount = 0;
            int cancelCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => quitCount++, () => cancelCount++));
            ConfirmQuitScreen quitConfirmation = RequireSingleScreen<ConfirmQuitScreen>(sut);
            Button yesButton = RequireButton(quitConfirmation, "Yes");
            Button noButton = RequireButton(quitConfirmation, "No");
            foreach (Button button in new[] { yesButton, noButton })
            {
                Assert.That(button.isActiveAndEnabled, Is.True, "Every production quit confirmation button should be active and enabled.");
                Assert.That(button.interactable, Is.True, "Every production quit confirmation button should be interactable.");
            }

            // Act
            yesButton.onClick.Invoke();
            int quitCountAfterYes = quitCount;
            int cancelCountAfterYes = cancelCount;
            noButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "Showing the production quit confirmation should preserve the base screen.");
            Assert.That(quitConfirmation.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production quit confirmation.");
            Assert.That(quitCountAfterYes, Is.EqualTo(1), "The production quit confirmation's Yes button should invoke the supplied quit action exactly once.");
            Assert.That(cancelCountAfterYes, Is.EqualTo(0), "The production quit confirmation's Yes button should not invoke the supplied cancel action.");
            Assert.That(quitCount, Is.EqualTo(1), "The production quit confirmation's No button should not invoke the supplied quit action.");
            Assert.That(cancelCount, Is.EqualTo(1), "The production quit confirmation's No button should invoke the supplied cancel action exactly once.");
        }

        [TestCase(Outcome.XWin, "X Wins!")]
        [TestCase(Outcome.OWin, "O Wins!")]
        [TestCase(Outcome.Draw, "Draw!")]
        public void Showing_each_terminal_production_outcome_role_over_a_base_presents_it_and_routes_the_continue_button(Outcome outcome, string expectedTitle)
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int continueCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IOutcomeScreen>(screen => screen.Setup(outcome, () => continueCount++));
            OutcomeScreen outcomeScreen = RequireSingleScreen<OutcomeScreen>(sut);
            Button continueButton = RequireButton(outcomeScreen, "Continue");
            Assert.That(continueButton.isActiveAndEnabled, Is.True, "The production Outcome screen's Continue button should be active and enabled.");
            Assert.That(continueButton.interactable, Is.True, "The production Outcome screen's Continue button should be interactable.");

            // Act
            continueButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "Showing the production outcome should preserve the base screen.");
            Assert.That(outcomeScreen.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production Outcome screen.");
            Assert.That(
                VisibleTexts(outcomeScreen),
                Is.EquivalentTo(new[] { "Continue", expectedTitle }),
                "The production Outcome screen should present the terminal outcome and its Continue control as its complete visible state.");
            Assert.That(continueCount, Is.EqualTo(1), "The production Outcome screen's Continue button should invoke the supplied action exactly once.");
        }

        private AppUI CreateProductionAppUI()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(APP_UI_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, $"The production prefab must exist at {APP_UI_PREFAB_PATH}.");

            GameObject instance = Object.Instantiate(prefab);
            _productionInstances.Add(instance);

            AppUI appUI = instance.GetComponent<AppUI>();
            Assert.That(appUI, Is.Not.Null, "The production application UI prefab must carry AppUI.");
            return appUI;
        }

        private static T RequireSingleScreen<T>(AppUI appUI) where T : Component
        {
            T[] screens = appUI.GetComponentsInChildren<T>(true);
            Assert.That(screens, Has.Length.EqualTo(1), $"The production application UI should show exactly one {typeof(T).Name} screen.");
            return screens[0];
        }

        private static Button RequireButton(Component screen, string label)
        {
            Button[] matchingButtons = screen.GetComponentsInChildren<Button>(true)
                .Where(button => LabelText(button) == label)
                .ToArray();
            Assert.That(
                matchingButtons,
                Has.Length.EqualTo(1),
                $"The production {screen.GetType().Name} screen should provide exactly one button labelled '{label}'.");
            return matchingButtons[0];
        }

        private static string LabelText(Button button)
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            return label == null ? string.Empty : label.text;
        }

        private static string[] VisibleTexts(Component screen)
        {
            return screen.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text.isActiveAndEnabled)
                .Select(text => text.text)
                .ToArray();
        }
    }
}
#endif
