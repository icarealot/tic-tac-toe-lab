#if UNITY_EDITOR
using TicTacToeLab.Runtime;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Typed accessors for the window hierarchy elements the scene tests reach. Every literal path
    /// into a window prefab lives here, and only here, so a prefab rename is answered in one file
    /// rather than in test after test.
    /// </summary>
    public static class WindowNodes
    {
        /// <summary>
        /// The button that answers the quit confirmation with yes. Both answers hang beneath the
        /// question card — the node under the popup's safe area that holds the question and its two
        /// answers, once named "Dialog", a term the glossary avoids for windows.
        /// </summary>
        public static Button YesButton(this ConfirmQuitPopup popup)
        {
            return popup.transform.Find("SafeArea/Question/YesButton").GetComponent<Button>();
        }

        /// <summary>The button that answers the quit confirmation with no.</summary>
        public static Button NoButton(this ConfirmQuitPopup popup)
        {
            return popup.transform.Find("SafeArea/Question/NoButton").GetComponent<Button>();
        }

        /// <summary>The gameplay panel's on-screen back button.</summary>
        public static Button BackButton(this GameplayPanel panel)
        {
            return panel.transform.Find("SafeArea/BackButton").GetComponent<Button>();
        }

        /// <summary>The main menu's title text.</summary>
        public static TMPro.TMP_Text Title(this MainMenuPanel panel)
        {
            return panel.transform.Find("SafeArea/TitleText").GetComponent<TMPro.TMP_Text>();
        }
    }
}
#endif
