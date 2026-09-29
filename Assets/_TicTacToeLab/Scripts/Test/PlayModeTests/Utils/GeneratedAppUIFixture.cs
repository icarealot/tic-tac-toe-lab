#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Generates the application UI's base and popup screens and layers so isolated checks never load production assets.
    /// Every requested object is returned with the direct button and text references it was wired with, and screens
    /// that AppUI creates from these templates are observed through <see cref="Screens"/>.
    /// The generated application UI can also serve as Bootstrap's application UI adapter template.
    /// </summary>
    public sealed class GeneratedAppUIFixture
    {
        public GeneratedScreenRegistry Screens { get; }
        public RectTransform BaseLayer { get; private set; }
        public RectTransform PopupLayer { get; private set; }

        private readonly List<GameObject> _generatedRoots = new();

        public GeneratedAppUIFixture()
        {
            GameObject registryRoot = CreateRoot("Generated screen registry");
            Screens = registryRoot.AddComponent<GeneratedScreenRegistry>();
        }

        public (HomeScreen Screen, Button PvpButton, Button PveButton) CreateHomeScreen()
        {
            GameObject root = CreateRoot("Generated Home screen");
            Button pvpButton = CreateButton(root.transform, "PvP button");
            Button pveButton = CreateButton(root.transform, "PvE button");
            HomeScreen screen = root.AddComponent<HomeScreen>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(screen, "_startButton", pvpButton);
            TestSerializedReference.AssignPrefab(screen, "_pveButton", pveButton);
            root.SetActive(true);
            return (screen, pvpButton, pveButton);
        }

        public (BotSelectionScreen Screen, Button AmateurButton, Button ProfessionalButton, Button BackButton) CreateBotSelectionScreen()
        {
            GameObject root = CreateRoot("Generated bot selection screen");
            Button amateurButton = CreateLabeledButton(root.transform, "Amateur button", "Amateur");
            Button professionalButton = CreateLabeledButton(root.transform, "Professional button", "Professional");
            Button backButton = CreateLabeledButton(root.transform, "Back button", "Back");
            BotSelectionScreen screen = root.AddComponent<BotSelectionScreen>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(screen, "_amateurButton", amateurButton);
            TestSerializedReference.AssignPrefab(screen, "_professionalButton", professionalButton);
            TestSerializedReference.AssignPrefab(screen, "_backButton", backButton);
            root.SetActive(true);
            return (screen, amateurButton, professionalButton, backButton);
        }

        public (GameplayScreen Screen, Button BackButton, TMP_Text TurnText) CreateGameplayScreen()
        {
            GameObject root = CreateRoot("Generated gameplay screen");
            Button backButton = CreateButton(root.transform, "Back button");
            TMP_Text turnText = CreateText(root.transform, "Turn text");
            GameplayScreen screen = root.AddComponent<GameplayScreen>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(screen, "_backButton", backButton);
            TestSerializedReference.AssignPrefab(screen, "_turnText", turnText);
            root.SetActive(true);
            return (screen, backButton, turnText);
        }

        public (ConfirmQuitScreen Screen, Button YesButton, Button NoButton) CreateConfirmQuitScreen()
        {
            GameObject root = CreateRoot("Generated confirm quit screen");
            Button yesButton = CreateButton(root.transform, "Yes button");
            Button noButton = CreateButton(root.transform, "No button");
            ConfirmQuitScreen screen = root.AddComponent<ConfirmQuitScreen>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(screen, "_yesButton", yesButton);
            TestSerializedReference.AssignPrefab(screen, "_noButton", noButton);
            root.SetActive(true);
            return (screen, yesButton, noButton);
        }

        public (OutcomeScreen Screen, Button ContinueButton, TMP_Text TitleText) CreateOutcomeScreen()
        {
            GameObject root = CreateRoot("Generated outcome screen");
            Button continueButton = CreateButton(root.transform, "Continue button");
            TMP_Text titleText = CreateText(root.transform, "Title text");
            OutcomeScreen screen = root.AddComponent<OutcomeScreen>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(screen, "_continueButton", continueButton);
            TestSerializedReference.AssignPrefab(screen, "_titleText", titleText);
            root.SetActive(true);
            return (screen, continueButton, titleText);
        }

        public T CreateScreenTemplate<T>() where T : MonoBehaviour
        {
            GameObject root = CreateRoot($"Generated {typeof(T).Name}");
            T screen = root.AddComponent<T>();
            AttachLifecycleProbe(root);
            root.SetActive(true);
            IgnoreAsTemplate(root);
            return screen;
        }

        public AppUI CreateInactiveAppUI(params ScreenRegistration[] registrations)
        {
            GameObject root = CreateRoot("Generated application UI");
            BaseLayer = CreateLayer(root.transform, "Generated base layer");
            PopupLayer = CreateLayer(root.transform, "Generated popup layer");

            AppUI appUI = root.AddComponent<AppUI>();
            TestSerializedReference.AssignPrefab(appUI, "_baseLayer", BaseLayer);
            TestSerializedReference.AssignPrefab(appUI, "_popupLayer", PopupLayer);
            TestSerializedReference.AssignScreenRegistrations(appUI, registrations);
            return appUI;
        }

        public AppUI CreateAppUI()
        {
            GameObject root = CreateRoot("Generated application UI");
            BaseLayer = CreateLayer(root.transform, "Generated base layer");
            PopupLayer = CreateLayer(root.transform, "Generated popup layer");

            (HomeScreen home, Button _, Button _) = CreateHomeScreen();
            (BotSelectionScreen botSelection, Button _, Button _, Button _) = CreateBotSelectionScreen();
            (GameplayScreen gameplay, Button _, TMP_Text _) = CreateGameplayScreen();
            (ConfirmQuitScreen quitConfirmation, Button _, Button _) = CreateConfirmQuitScreen();
            (OutcomeScreen outcome, Button _, TMP_Text _) = CreateOutcomeScreen();

            IgnoreAsTemplate(home.gameObject);
            IgnoreAsTemplate(botSelection.gameObject);
            IgnoreAsTemplate(gameplay.gameObject);
            IgnoreAsTemplate(quitConfirmation.gameObject);
            IgnoreAsTemplate(outcome.gameObject);

            AppUI appUI = root.AddComponent<AppUI>();
            TestSerializedReference.AssignPrefab(appUI, "_baseLayer", BaseLayer);
            TestSerializedReference.AssignPrefab(appUI, "_popupLayer", PopupLayer);
            TestSerializedReference.AssignScreenRegistrations(
                appUI,
                new[]
                {
                    new ScreenRegistration(home, ScreenLayer.Base),
                    new ScreenRegistration(botSelection, ScreenLayer.Base),
                    new ScreenRegistration(gameplay, ScreenLayer.Base),
                    new ScreenRegistration(quitConfirmation, ScreenLayer.Popup),
                    new ScreenRegistration(outcome, ScreenLayer.Popup),
                });
            root.SetActive(true);
            return appUI;
        }

        public IEnumerator IE_DestroyAll()
        {
            GameObject[] generatedRoots = _generatedRoots.ToArray();
            _generatedRoots.Clear();

            foreach (GameObject generatedRoot in generatedRoots)
            {
                if (generatedRoot != null)
                {
                    Object.Destroy(generatedRoot);
                }
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => generatedRoots.All(generatedRoot => generatedRoot == null),
                "The generated application UI objects should be destroyed after the test.");
        }

        private GameObject CreateRoot(string name)
        {
            GameObject root = new(name, typeof(RectTransform));
            root.SetActive(false);
            _generatedRoots.Add(root);
            return root;
        }

        private void AttachLifecycleProbe(GameObject screenRoot)
        {
            LifecycleProbe probe = screenRoot.AddComponent<LifecycleProbe>();
            TestSerializedReference.AssignPrefab(probe, "_registry", Screens);
        }

        private static void IgnoreAsTemplate(GameObject screenRoot)
        {
            screenRoot.GetComponent<LifecycleProbe>().IgnoreAsTemplate();
        }

        private static RectTransform CreateLayer(Transform parent, string name)
        {
            GameObject layerRoot = new(name, typeof(RectTransform));
            layerRoot.transform.SetParent(parent, worldPositionStays: false);
            return (RectTransform)layerRoot.transform;
        }

        private static Button CreateButton(Transform parent, string name)
        {
            GameObject buttonRoot = new(name, typeof(RectTransform));
            buttonRoot.transform.SetParent(parent, worldPositionStays: false);
            return buttonRoot.AddComponent<Button>();
        }

        private static Button CreateLabeledButton(Transform parent, string name, string label)
        {
            Button button = CreateButton(parent, name);
            TMP_Text text = CreateText(button.transform, label);
            text.SetText(label);
            return button;
        }

        private static TMP_Text CreateText(Transform parent, string name)
        {
            GameObject textRoot = new(name, typeof(RectTransform));
            textRoot.transform.SetParent(parent, worldPositionStays: false);
            return textRoot.AddComponent<TextMeshProUGUI>();
        }
    }
}
#endif
