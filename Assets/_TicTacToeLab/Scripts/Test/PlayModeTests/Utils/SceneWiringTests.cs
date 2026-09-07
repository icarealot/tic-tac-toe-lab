#if UNITY_EDITOR
using System.Collections;
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
    }
}
#endif
