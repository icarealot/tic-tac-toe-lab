#if UNITY_EDITOR
using System.Collections;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// A play-mode test that loads the real scene. Every such test shares this fixture, because the
    /// app enables input actions the moment it comes up: one that ran outside the input fixture's
    /// sandbox would leave those actions bound to devices the next test's reset takes away, and the
    /// throw would land in that next test rather than in the one that caused it.
    /// </summary>
    public abstract class SceneWiringTests : InputTestFixture
    {
        protected const string SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";

        protected IEnumerator IE_LoadScene()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(
                SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        protected IEnumerator IE_PressCell(Mouse mouse, Transform cellTransform)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(cellTransform.position);

            Set(mouse.position, new Vector2(screenPoint.x, screenPoint.y));
            Press(mouse.leftButton);
            Release(mouse.leftButton);

            yield return null;
        }

        protected IEnumerator IE_ClickButton(Mouse mouse, Button button)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);

            Set(mouse.position, screenPoint);
            yield return null;
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);

            // The button raises onClick on release, and the closed window is destroyed at end of frame.
            yield return null;
            yield return null;
        }

        // The app opens on the main menu, so every test that plays a game begins by pressing Start.
        protected IEnumerator IE_StartGame(Mouse mouse)
        {
            Button startButton = Object.FindFirstObjectByType<MainMenuPanel>().GetComponentInChildren<Button>();

            yield return IE_ClickButton(mouse, startButton);
        }
    }
}
#endif
