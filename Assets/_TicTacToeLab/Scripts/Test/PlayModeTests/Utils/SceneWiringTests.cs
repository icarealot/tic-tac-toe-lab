#if UNITY_EDITOR
using System.Collections;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TicTacToeLab.PlayModeTests
{
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

        protected IEnumerator IE_ClickButton(Mouse mouse, UnityEngine.UI.Button button)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) / 2f);

            yield return IE_ClickScreenPoint(mouse, screenPoint);
        }

        protected IEnumerator IE_ClickScreenPoint(Mouse mouse, Vector2 screenPoint)
        {
            Set(mouse.position, screenPoint);
            yield return null;
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);

            yield return null;
            yield return null;
        }

        protected IEnumerator IE_PressBack(Keyboard keyboard)
        {
            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);

            yield return null;
        }

        protected IEnumerator IE_StartGame(Mouse mouse)
        {
            UnityEngine.UI.Button startButton = Object.FindFirstObjectByType<MainMenuPanel>()
                                                        .GetComponentInChildren<UnityEngine.UI.Button>();

            yield return IE_ClickButton(mouse, startButton);
        }
    }
}
#endif
