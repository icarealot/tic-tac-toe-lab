#if UNITY_EDITOR
using System.Collections;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// A play-mode test that loads the real scene. Every such test shares this fixture, because the
    /// app enables input actions the moment it comes up: one that ran outside the input fixture's
    /// sandbox would leave those actions bound to devices the next test's reset takes away, and the
    /// throw would land in that next test rather than in the one that caused it.
    /// <para>
    /// All input simulation — press, button activation, back — goes through the helpers below, and
    /// each helper owns its frame-yield policy: it yields the frames its gesture needs to land, and
    /// when the gesture closes a window it waits one frame further, because the closed window is
    /// destroyed at the end of the frame that closes it. Change simulation timing here, in one
    /// place — never in a test.
    /// </para>
    /// </summary>
    public abstract class SceneWiringTests : InputTestFixture
    {
        protected const string SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";

        /// <summary>
        /// Loads the real scene and yields one frame for its startup — the input actions coming up —
        /// to settle.
        /// </summary>
        protected IEnumerator IE_LoadScene()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(
                SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        /// <summary>
        /// Simulates a press on the board: aims the mouse at the cell's screen point, pushes down
        /// and releases in the same frame, then yields one frame for the mark to land. A press
        /// destroys nothing, so no destruction wait applies.
        /// </summary>
        protected IEnumerator IE_PressCell(Mouse mouse, Transform cellTransform)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(cellTransform.position);

            Set(mouse.position, new Vector2(screenPoint.x, screenPoint.y));
            Press(mouse.leftButton);
            Release(mouse.leftButton);

            yield return null;
        }

        /// <summary>
        /// Simulates a button activation through the Unity event system: aims the mouse at the
        /// middle of the button's rect — not its transform position, which is the rect's center only
        /// for a centered pivot, while the back button, for one, hangs from its top-left corner —
        /// then yields for the hover, presses, yields for the press, and releases. The button
        /// raises onClick on release, and a window the activation closes is destroyed at the end of
        /// that frame; the two trailing yields cover the release and that end-of-frame destruction.
        /// </summary>
        protected IEnumerator IE_ClickButton(Mouse mouse, UnityEngine.UI.Button button)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) / 2f);

            Set(mouse.position, screenPoint);
            yield return null;
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);

            yield return null;
            yield return null;
        }

        /// <summary>
        /// Simulates the back gesture: presses and releases the keyboard's escape key across frames.
        /// The asking state closes its window on the press, and the closed window is destroyed at
        /// the end of that frame; the final yield waits out that end-of-frame destruction.
        /// </summary>
        protected IEnumerator IE_PressBack(Keyboard keyboard)
        {
            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);

            yield return null;
        }

        // The app opens on the main menu, so every test that plays a game begins by pressing Start.
        protected IEnumerator IE_StartGame(Mouse mouse)
        {
            UnityEngine.UI.Button startButton = Object.FindFirstObjectByType<MainMenuPanel>()
                .GetComponentInChildren<UnityEngine.UI.Button>();

            yield return IE_ClickButton(mouse, startButton);
        }
    }
}
#endif
