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
    /// Generates the application UI's panels, popups, and layers so isolated checks never load production assets.
    /// Every requested object is returned with the direct button and text references it was wired with, and windows
    /// that AppUI creates from these templates are observed through <see cref="Windows"/>.
    /// The generated application UI can also serve as Bootstrap's application UI adapter template.
    /// </summary>
    public sealed class GeneratedAppUIFixture
    {
        public GeneratedWindowRegistry Windows { get; }

        private readonly List<GameObject> _generatedRoots = new();

        public GeneratedAppUIFixture()
        {
            GameObject registryRoot = CreateRoot("Generated window registry");
            Windows = registryRoot.AddComponent<GeneratedWindowRegistry>();
        }

        public (MainMenuPanel Panel, Button StartButton) CreateMainMenuPanel()
        {
            GameObject root = CreateRoot("Generated main menu panel");
            Button startButton = CreateButton(root.transform, "Start button");
            MainMenuPanel panel = root.AddComponent<MainMenuPanel>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(panel, "_startButton", startButton);
            root.SetActive(true);
            return (panel, startButton);
        }

        public (GameplayPanel Panel, Button BackButton, TMP_Text TurnText) CreateGameplayPanel()
        {
            GameObject root = CreateRoot("Generated gameplay panel");
            Button backButton = CreateButton(root.transform, "Back button");
            TMP_Text turnText = CreateText(root.transform, "Turn text");
            GameplayPanel panel = root.AddComponent<GameplayPanel>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(panel, "_backButton", backButton);
            TestSerializedReference.AssignPrefab(panel, "_turnText", turnText);
            root.SetActive(true);
            return (panel, backButton, turnText);
        }

        public (ConfirmQuitPopup Popup, Button YesButton, Button NoButton) CreateQuitConfirmationPopup()
        {
            GameObject root = CreateRoot("Generated quit confirmation popup");
            Button yesButton = CreateButton(root.transform, "Yes button");
            Button noButton = CreateButton(root.transform, "No button");
            ConfirmQuitPopup popup = root.AddComponent<ConfirmQuitPopup>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(popup, "_yesButton", yesButton);
            TestSerializedReference.AssignPrefab(popup, "_noButton", noButton);
            root.SetActive(true);
            return (popup, yesButton, noButton);
        }

        public (OutcomePopup Popup, Button ContinueButton, TMP_Text TitleText) CreateOutcomePopup()
        {
            GameObject root = CreateRoot("Generated outcome popup");
            Button continueButton = CreateButton(root.transform, "Continue button");
            TMP_Text titleText = CreateText(root.transform, "Title text");
            OutcomePopup popup = root.AddComponent<OutcomePopup>();
            AttachLifecycleProbe(root);
            TestSerializedReference.AssignPrefab(popup, "_continueButton", continueButton);
            TestSerializedReference.AssignPrefab(popup, "_titleText", titleText);
            root.SetActive(true);
            return (popup, continueButton, titleText);
        }

        public AppUI CreateAppUI()
        {
            GameObject root = CreateRoot("Generated application UI");
            RectTransform panelLayer = CreateLayer(root.transform, "Generated panel layer");
            RectTransform popupLayer = CreateLayer(root.transform, "Generated popup layer");

            (MainMenuPanel mainMenu, Button _) = CreateMainMenuPanel();
            (GameplayPanel gameplay, Button _, TMP_Text _) = CreateGameplayPanel();
            (ConfirmQuitPopup quitConfirmation, Button _, Button _) = CreateQuitConfirmationPopup();
            (OutcomePopup outcome, Button _, TMP_Text _) = CreateOutcomePopup();

            IgnoreAsTemplate(mainMenu.gameObject);
            IgnoreAsTemplate(gameplay.gameObject);
            IgnoreAsTemplate(quitConfirmation.gameObject);
            IgnoreAsTemplate(outcome.gameObject);

            AppUI appUI = root.AddComponent<AppUI>();
            TestSerializedReference.AssignPrefab(appUI, "_mainMenuPanelPrefab", mainMenu);
            TestSerializedReference.AssignPrefab(appUI, "_gameplayPanelPrefab", gameplay);
            TestSerializedReference.AssignPrefab(appUI, "_quitConfirmationPopupPrefab", quitConfirmation);
            TestSerializedReference.AssignPrefab(appUI, "_outcomePopupPrefab", outcome);
            TestSerializedReference.AssignPrefab(appUI, "_panelLayer", panelLayer);
            TestSerializedReference.AssignPrefab(appUI, "_popupLayer", popupLayer);
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

        private void AttachLifecycleProbe(GameObject windowRoot)
        {
            LifecycleProbe probe = windowRoot.AddComponent<LifecycleProbe>();
            TestSerializedReference.AssignPrefab(probe, "_registry", Windows);
        }

        private static void IgnoreAsTemplate(GameObject windowRoot)
        {
            windowRoot.GetComponent<LifecycleProbe>().IgnoreAsTemplate();
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

        private static TMP_Text CreateText(Transform parent, string name)
        {
            GameObject textRoot = new(name, typeof(RectTransform));
            textRoot.transform.SetParent(parent, worldPositionStays: false);
            return textRoot.AddComponent<TextMeshProUGUI>();
        }
    }
}
#endif
