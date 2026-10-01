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
        public void Showing_the_production_home_role_routes_its_PvP_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int pvpCount = 0;
            int pveCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => pvpCount++, () => pveCount++));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            Button pvpButton = RequireButton(home, "PvP");
            Button pveButton = RequireButton(home, "PvE");
            Assert.That(pvpButton.isActiveAndEnabled, Is.True, "The production Home screen's PvP button should be active and enabled.");
            Assert.That(pvpButton.interactable, Is.True, "The production Home screen's PvP button should be interactable.");
            Assert.That(
                pvpButton.onClick.GetPersistentEventCount(),
                Is.EqualTo(0),
                "The production Home screen's PvP button should have no persistent listeners.");
            Assert.That(
                pveButton.onClick.GetPersistentEventCount(),
                Is.EqualTo(0),
                "The production Home screen's PvE button should have no persistent listeners.");

            // Act
            pvpButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production Home screen.");
            Assert.That(pvpCount, Is.EqualTo(1), "The production Home screen's PvP button should invoke the supplied action exactly once.");
            Assert.That(pveCount, Is.EqualTo(0), "The production Home screen's PvP button should not invoke the PvE action.");
        }

        [Test]
        public void Showing_the_production_home_role_routes_its_PvE_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int pvpCount = 0;
            int pveCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => pvpCount++, () => pveCount++));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            Button pvpButton = RequireButton(home, "PvP");
            Button pveButton = RequireButton(home, "PvE");
            Assert.That(pveButton.isActiveAndEnabled, Is.True, "The production Home screen's PvE button should be active and enabled.");
            Assert.That(pveButton.interactable, Is.True, "The production Home screen's PvE button should be interactable.");
            Assert.That(
                pvpButton.onClick.GetPersistentEventCount(),
                Is.EqualTo(0),
                "The production Home screen's PvP button should have no persistent listeners.");
            Assert.That(
                pveButton.onClick.GetPersistentEventCount(),
                Is.EqualTo(0),
                "The production Home screen's PvE button should have no persistent listeners.");

            // Act
            pveButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production Home screen.");
            Assert.That(pveCount, Is.EqualTo(1), "The production Home screen's PvE button should invoke the supplied action exactly once.");
            Assert.That(pvpCount, Is.EqualTo(0), "The production Home screen's PvE button should not invoke the PvP action.");
        }

        [Test]
        public void Showing_the_production_bot_selection_role_routes_its_Amateur_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IBotSelectionScreen>(screen => screen.Setup(() => amateurCount++, () => professionalCount++, () => backCount++));
            BotSelectionScreen botSelection = RequireSingleScreen<BotSelectionScreen>(sut);
            Button amateurButton = RequireButton(botSelection, "Amateur");
            _ = RequireButton(botSelection, "Professional");
            _ = RequireButton(botSelection, "Back");
            Assert.That(amateurButton.isActiveAndEnabled, Is.True, "The production bot selection's Amateur button should be active and enabled.");
            Assert.That(amateurButton.interactable, Is.True, "The production bot selection's Amateur button should be interactable.");

            // Act
            amateurButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.False, "Showing bot selection should replace the production Home base screen.");
            Assert.That(botSelection.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production base screen.");
            Assert.That(amateurCount, Is.EqualTo(1), "The production bot selection's Amateur button should invoke the supplied action exactly once.");
            Assert.That(professionalCount, Is.EqualTo(0), "The production bot selection's Amateur button should not invoke the Professional action.");
            Assert.That(backCount, Is.EqualTo(0), "The production bot selection's Amateur button should not invoke the Back action.");
        }

        [Test]
        public void Showing_the_production_bot_selection_role_routes_its_Professional_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IBotSelectionScreen>(screen => screen.Setup(() => amateurCount++, () => professionalCount++, () => backCount++));
            BotSelectionScreen botSelection = RequireSingleScreen<BotSelectionScreen>(sut);
            _ = RequireButton(botSelection, "Amateur");
            Button professionalButton = RequireButton(botSelection, "Professional");
            _ = RequireButton(botSelection, "Back");
            Assert.That(professionalButton.isActiveAndEnabled, Is.True, "The production bot selection's Professional button should be active and enabled.");
            Assert.That(professionalButton.interactable, Is.True, "The production bot selection's Professional button should be interactable.");

            // Act
            professionalButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.False, "Showing bot selection should replace the production Home base screen.");
            Assert.That(botSelection.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production base screen.");
            Assert.That(amateurCount, Is.EqualTo(0), "The production bot selection's Professional button should not invoke the Amateur action.");
            Assert.That(professionalCount, Is.EqualTo(1), "The production bot selection's Professional button should invoke the supplied action exactly once.");
            Assert.That(backCount, Is.EqualTo(0), "The production bot selection's Professional button should not invoke the Back action.");
        }

        [Test]
        public void Showing_the_production_bot_selection_role_routes_its_Back_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int amateurCount = 0;
            int professionalCount = 0;
            int backCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IBotSelectionScreen>(screen => screen.Setup(() => amateurCount++, () => professionalCount++, () => backCount++));
            BotSelectionScreen botSelection = RequireSingleScreen<BotSelectionScreen>(sut);
            _ = RequireButton(botSelection, "Amateur");
            _ = RequireButton(botSelection, "Professional");
            Button backButton = RequireButton(botSelection, "Back");
            Assert.That(backButton.isActiveAndEnabled, Is.True, "The production bot selection's Back button should be active and enabled.");
            Assert.That(backButton.interactable, Is.True, "The production bot selection's Back button should be interactable.");

            // Act
            backButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.False, "Showing bot selection should replace the production Home base screen.");
            Assert.That(botSelection.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production base screen.");
            Assert.That(amateurCount, Is.EqualTo(0), "The production bot selection's Back button should not invoke the Amateur action.");
            Assert.That(professionalCount, Is.EqualTo(0), "The production bot selection's Back button should not invoke the Professional action.");
            Assert.That(backCount, Is.EqualTo(1), "The production bot selection's Back button should invoke the supplied action exactly once.");
        }

        [Test]
        public void Showing_the_production_gameplay_role_replaces_the_base_screen_and_routes_its_back_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            BoardPresenterHarness boardPresenterHarness = new();
            int backCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IGameplayScreen>(screen => screen.Setup(boardPresenterHarness.Presenter, new GameSetup(GameMode.Pvp, null), () => backCount++));
            GameplayScreen gameplay = RequireSingleScreen<GameplayScreen>(sut);
            Button backButton = RequireButton(gameplay, "Back");
            Assert.That(backButton.isActiveAndEnabled, Is.True, "The production gameplay's Back button should be active and enabled.");
            Assert.That(backButton.interactable, Is.True, "The production gameplay's Back button should be interactable.");

            // Act
            backButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.False, "The production Gameplay registration should replace the active base screen.");
            Assert.That(gameplay.gameObject.activeInHierarchy, Is.True, "The production application UI should show production gameplay.");
            Assert.That(backCount, Is.EqualTo(1), "The production gameplay's Back button should invoke the supplied action exactly once.");
        }

        [Test]
        public void Showing_the_production_confirm_quit_role_routes_its_Yes_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int quitCount = 0;
            int cancelCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => quitCount++, () => cancelCount++));
            ConfirmQuitScreen quitConfirmation = RequireSingleScreen<ConfirmQuitScreen>(sut);
            Button yesButton = RequireButton(quitConfirmation, "Yes");
            _ = RequireButton(quitConfirmation, "No");
            Assert.That(yesButton.isActiveAndEnabled, Is.True, "The production quit confirmation's Yes button should be active and enabled.");
            Assert.That(yesButton.interactable, Is.True, "The production quit confirmation's Yes button should be interactable.");

            // Act
            yesButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "Showing the production quit confirmation should preserve the base screen.");
            Assert.That(quitConfirmation.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production quit confirmation.");
            Assert.That(quitCount, Is.EqualTo(1), "The production quit confirmation's Yes button should invoke the supplied quit action exactly once.");
            Assert.That(cancelCount, Is.EqualTo(0), "The production quit confirmation's Yes button should not invoke the supplied cancel action.");
        }

        [Test]
        public void Showing_the_production_confirm_quit_role_routes_its_No_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int quitCount = 0;
            int cancelCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => quitCount++, () => cancelCount++));
            ConfirmQuitScreen quitConfirmation = RequireSingleScreen<ConfirmQuitScreen>(sut);
            _ = RequireButton(quitConfirmation, "Yes");
            Button noButton = RequireButton(quitConfirmation, "No");
            Assert.That(noButton.isActiveAndEnabled, Is.True, "The production quit confirmation's No button should be active and enabled.");
            Assert.That(noButton.interactable, Is.True, "The production quit confirmation's No button should be interactable.");

            // Act
            noButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "Showing the production quit confirmation should preserve the base screen.");
            Assert.That(quitConfirmation.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production quit confirmation.");
            Assert.That(quitCount, Is.EqualTo(0), "The production quit confirmation's No button should not invoke the supplied quit action.");
            Assert.That(cancelCount, Is.EqualTo(1), "The production quit confirmation's No button should invoke the supplied cancel action exactly once.");
        }

        [Test]
        public void Showing_the_production_outcome_role_over_a_base_routes_the_continue_button()
        {
            // Arrange
            AppUI sut = CreateProductionAppUI();
            int continueCount = 0;
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }, () => { }));
            HomeScreen home = RequireSingleScreen<HomeScreen>(sut);
            sut.Show<IOutcomeScreen>(screen => screen.Setup(Outcome.XWin, new GameSetup(GameMode.Pvp, null), () => continueCount++));
            OutcomeScreen outcomeScreen = RequireSingleScreen<OutcomeScreen>(sut);
            Button continueButton = RequireButton(outcomeScreen, "Continue");
            Assert.That(continueButton.isActiveAndEnabled, Is.True, "The production Outcome screen's Continue button should be active and enabled.");
            Assert.That(continueButton.interactable, Is.True, "The production Outcome screen's Continue button should be interactable.");

            // Act
            continueButton.onClick.Invoke();

            // Assert
            Assert.That(home.gameObject.activeInHierarchy, Is.True, "Showing the production outcome should preserve the base screen.");
            Assert.That(outcomeScreen.gameObject.activeInHierarchy, Is.True, "The production application UI should show the production Outcome screen.");
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
    }
}
#endif
