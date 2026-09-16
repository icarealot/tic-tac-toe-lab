#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace TicTacToeLab.PlayModeTests
{
    public abstract class SceneWiringTests : InputTestFixture
    {
        protected const string SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";

        private const float ASYNC_OBSERVATION_TIMEOUT_SECONDS = 3f;

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
            InputSystem.Update();
            Press(mouse.leftButton);
            InputSystem.Update();
            Release(mouse.leftButton);
            InputSystem.Update();

            yield return null;
        }

        protected IEnumerator IE_ClickButton(Mouse mouse, UnityEngine.UI.Button button)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) / 2f);
            InputSystemUIInputModule uiInputModule = Object.FindFirstObjectByType<InputSystemUIInputModule>();
            Assert.That(uiInputModule, Is.Not.Null, "The production scene should provide an Input System UI module.");

            Set(mouse.position, screenPoint);
            InputSystem.Update();
            uiInputModule.Process();

            Press(mouse.leftButton);
            InputSystem.Update();
            uiInputModule.Process();

            Release(mouse.leftButton);
            InputSystem.Update();
            uiInputModule.Process();

            yield return null;
        }

        protected IEnumerator IE_PressBack(Keyboard keyboard)
        {
            Press(keyboard.escapeKey);
            InputSystem.Update();
            Release(keyboard.escapeKey);
            InputSystem.Update();

            yield return null;
        }

        protected IEnumerator IE_StartGameThroughEventSystem(Mouse mouse)
        {
            yield return IE_ClickButton(mouse, StartButton());
            yield return IE_WaitForGameplay();
        }

        protected IEnumerator IE_StartGameThroughButtonEvent()
        {
            StartButton().onClick.Invoke();
            yield return IE_WaitForGameplay();
        }

        protected static IEnumerator IE_WaitUntil(System.Func<bool> condition, string failureMessage)
        {
            float deadline = Time.realtimeSinceStartup + ASYNC_OBSERVATION_TIMEOUT_SECONDS;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(condition(), Is.True, failureMessage);
        }

        private static UnityEngine.UI.Button StartButton()
        {
            return Object.FindFirstObjectByType<MainMenuPanel>().StartButton;
        }

        private static IEnumerator IE_WaitForGameplay()
        {
            yield return IE_WaitUntil(
                () => Object.FindFirstObjectByType<MainMenuPanel>() == null && Object.FindFirstObjectByType<GameplayPanel>() != null,
                "Start should close the main menu and show gameplay.");
        }
    }
}
#endif
