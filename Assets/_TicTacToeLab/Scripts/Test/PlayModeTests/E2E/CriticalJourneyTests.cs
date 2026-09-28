#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CriticalJourneyTests : InputTestFixture
    {
        private const string MAIN_SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";
        private const int BOARD_DIMENSION = 3;
        private const int WINNING_ROW = 2;
        private const string X_TURN_TEXT = "X's turn";
        private const string X_WIN_TEXT = "X Wins!";

        // X plays first, so the first, third, and fifth presses complete the chosen row while O answers between them.
        private static readonly CellCoordinate[] JOURNEY_PRESSES =
        {
            new(WINNING_ROW, 0),
            new(1, 0),
            new(WINNING_ROW, 1),
            new(1, 1),
            new(WINNING_ROW, 2),
        };

        private Mouse _mouse;
        private Scene _previousActiveScene;
        private Bootstrap _bootstrap;
        private Camera _camera;
        private BoardView _boardView;
        private AppUI _appUI;
        private DelayScheduler _delayScheduler;
        private InputActionAsset[] _journeyInputAssets = new InputActionAsset[0];

        public override void Setup()
        {
            base.Setup();
            _mouse = InputSystem.AddDevice<Mouse>();
        }

        [UnityTearDown]
        public IEnumerator DestroyJourneyComposition()
        {
            Bootstrap bootstrap = _bootstrap;
            _bootstrap = null;
            if (bootstrap != null)
            {
                Object.Destroy(bootstrap.gameObject);
            }

            Component[] ownedRoots = { _camera, _boardView, _appUI, _delayScheduler };
            _camera = null;
            _boardView = null;
            _appUI = null;
            _delayScheduler = null;

            InputActionAsset[] journeyInputAssets = _journeyInputAssets;
            _journeyInputAssets = new InputActionAsset[0];

            // Resolve the scene from its path so cleanup also runs when the test failed before capturing the composition.
            Scene journeyScene = SceneManager.GetSceneByPath(MAIN_SCENE_PATH);
            if (_previousActiveScene.IsValid() && _previousActiveScene.isLoaded)
            {
                _ = SceneManager.SetActiveScene(_previousActiveScene);
            }

            _previousActiveScene = default;

            if (journeyScene.IsValid() && journeyScene.isLoaded)
            {
                _ = SceneManager.UnloadSceneAsync(journeyScene);
            }

            if (journeyScene.IsValid())
            {
                yield return PlayModeWait.IE_WaitUntilOrFail(
                    () => !journeyScene.isLoaded,
                    "The production Main scene should unload after the journey.");
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ownedRoots.All(root => root == null),
                "Bootstrap should destroy every adapter root it instantiated when the journey is over.");

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => journeyInputAssets.All(asset => asset == null),
                "Bootstrap should release the Input System actions created for the journey.");
        }

        [UnityTest]
        public IEnumerator The_production_journey_runs_from_Home_through_an_x_win_and_back()
        {
            // Arrange
            Assert.That(
                EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == MAIN_SCENE_PATH),
                Is.True,
                $"The production scene at {MAIN_SCENE_PATH} should be enabled so the journey runs the shipped entry scene.");
            _previousActiveScene = SceneManager.GetActiveScene();
            InputActionAsset[] inputAssetsBeforeJourney = Resources.FindObjectsOfTypeAll<InputActionAsset>();

            // Act
            AsyncOperation load = SceneManager.LoadSceneAsync(MAIN_SCENE_PATH, LoadSceneMode.Additive);
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => load.isDone,
                $"The production scene at {MAIN_SCENE_PATH} should load with its Bootstrap composition.");
            CaptureJourneyComposition(SceneManager.GetSceneByPath(MAIN_SCENE_PATH), inputAssetsBeforeJourney);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => FindActiveScreen<HomeScreen>() != null,
                "The production scene should start with the production Home screen available to the player.");
            Button startButton = RequireButton(FindActiveScreen<HomeScreen>());
            Assert.That(startButton.isActiveAndEnabled, Is.True, "The production Home screen's Start button should be available to the player.");
            Assert.That(startButton.interactable, Is.True, "The production Home screen's Start button should be usable by the player.");

            // Act
            yield return IE_Click(startButton);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => FindActiveScreen<GameplayScreen>() != null && BoardMarks().Length == 0,
                "Clicking Start should show gameplay with an empty board.");
            GameplayScreen gameplayScreen = FindActiveScreen<GameplayScreen>();
            Assert.That(
                VisibleTexts(gameplayScreen),
                Has.Exactly(1).EqualTo(X_TURN_TEXT),
                "Gameplay should begin on X's turn.");

            // Act
            for (int pressIndex = 0; pressIndex < JOURNEY_PRESSES.Length; pressIndex++)
            {
                CellCoordinate coordinate = JOURNEY_PRESSES[pressIndex];
                Vector2 pressPoint = ScreenPointForCell(coordinate);
                PressBoardPoint(pressPoint);

                // Assert
                int expectedMarks = pressIndex + 1;
                yield return PlayModeWait.IE_WaitUntilOrFail(
                    () => BoardMarks().Length == expectedMarks,
                    $"Input System press {expectedMarks} should place a mark in cell {coordinate} on the production board.");
            }

            MarkView[] completedBoardMarks = BoardMarks();
            Assert.That(
                completedBoardMarks,
                Has.Length.EqualTo(JOURNEY_PRESSES.Length),
                "The completed board should own one production mark for every journey press.");
            Assert.That(
                ObservableMarksInRow(WINNING_ROW),
                Has.Length.EqualTo(BOARD_DIMENSION),
                "The completed row should own one observable production mark in every column of the winning row.");

            // Act & Assert: the outcome is presented asynchronously over the untouched completed board.
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => FindActiveScreen<OutcomeScreen>() != null
                    && completedBoardMarks.All(mark => mark != null && mark.gameObject.activeInHierarchy),
                "The outcome screen should appear while every completed-board mark remains observable.");
            OutcomeScreen outcomeScreen = FindActiveScreen<OutcomeScreen>();
            Assert.That(
                VisibleTexts(outcomeScreen),
                Has.Exactly(1).EqualTo(X_WIN_TEXT),
                "The outcome screen should report the X win.");
            Assert.That(
                completedBoardMarks.All(mark => mark != null && mark.gameObject.activeInHierarchy),
                Is.True,
                "Every completed-board mark should remain observable while the outcome screen is shown.");
            Assert.That(
                ObservableMarksInRow(WINNING_ROW),
                Has.Length.EqualTo(BOARD_DIMENSION),
                "The winning row should remain observable on the production board while the outcome screen is shown.");

            // Act
            yield return IE_Click(RequireButton(outcomeScreen));

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => FindActiveScreen<HomeScreen>() != null
                    && FindActiveScreen<GameplayScreen>() == null
                    && FindActiveScreen<OutcomeScreen>() == null,
                "Clicking Continue should return the player to Home.");
            Assert.That(
                RequireButton(FindActiveScreen<HomeScreen>()).interactable,
                Is.True,
                "The production Home screen should be usable again after the journey returns to it.");
        }

        private void CaptureJourneyComposition(Scene journeyScene, InputActionAsset[] inputAssetsBeforeJourney)
        {
            Assert.That(
                journeyScene.IsValid() && journeyScene.isLoaded,
                Is.True,
                "The enabled production Main scene should be loaded for the journey.");
            _bootstrap = journeyScene.GetRootGameObjects()
                .Select(root => root.GetComponent<Bootstrap>())
                .Single(component => component != null);
            _camera = RequireSingleAdapter<Camera>();
            _boardView = RequireSingleAdapter<BoardView>();
            _appUI = RequireSingleAdapter<AppUI>();
            _delayScheduler = RequireSingleAdapter<DelayScheduler>();

            _journeyInputAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>()
                .Where(asset => !inputAssetsBeforeJourney.Contains(asset))
                .ToArray();
            Assert.That(
                _journeyInputAssets,
                Has.Length.EqualTo(1),
                "The production composition should create exactly the input service's actions for the journey.");
        }

        private Vector2 ScreenPointForCell(CellCoordinate coordinate)
        {
            return _camera.WorldToScreenPoint(RequireCell(coordinate).transform.position);
        }

        private void PressBoardPoint(Vector2 screenPoint)
        {
            Set(_mouse.position, screenPoint, queueEventOnly: true);
            Press(_mouse.leftButton, queueEventOnly: true);
            InputSystem.Update();
            Release(_mouse.leftButton, queueEventOnly: true);
            InputSystem.Update();
        }

        private IEnumerator IE_Click(Button button)
        {
            Vector2 screenPoint = ScreenCenterOf(button);
            Set(_mouse.position, screenPoint, queueEventOnly: true);
            InputSystem.Update();
            yield return null;
            Press(_mouse.leftButton, queueEventOnly: true);
            InputSystem.Update();
            yield return null;
            Release(_mouse.leftButton, queueEventOnly: true);
            InputSystem.Update();
            yield return null;
        }

        private CellView RequireCell(CellCoordinate coordinate)
        {
            BoardLayout layout = new(BOARD_DIMENSION);
            CellView matchingCell = _boardView.GetComponentsInChildren<CellView>(true)
                .FirstOrDefault(cell =>
                {
                    if (cell == null)
                    {
                        return false;
                    }

                    Vector3 localPoint = _boardView.ToLocalPoint(cell.transform.position);
                    return layout.TryResolvePoint(localPoint, out CellCoordinate addressed) && addressed == coordinate;
                });

            Assert.That(
                matchingCell,
                Is.Not.Null,
                $"The production board should create a cell at cell coordinate ({coordinate.Row}, {coordinate.Column}).");
            return matchingCell;
        }

        private MarkView[] ObservableMarksInRow(int row)
        {
            return Enumerable.Range(0, BOARD_DIMENSION)
                .SelectMany(column => RequireCell(new CellCoordinate(row, column)).GetComponentsInChildren<MarkView>(true))
                .Where(mark => mark != null && mark.gameObject.activeInHierarchy)
                .ToArray();
        }

        private MarkView[] BoardMarks()
        {
            return _boardView.GetComponentsInChildren<MarkView>(true)
                .Where(mark => mark != null)
                .ToArray();
        }

        private T FindActiveScreen<T>() where T : Component
        {
            return _appUI.GetComponentsInChildren<T>(true)
                .FirstOrDefault(screen => screen != null && screen.gameObject.activeInHierarchy);
        }

        private static T RequireSingleAdapter<T>() where T : Component
        {
            T[] adapters = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.That(
                adapters,
                Has.Length.EqualTo(1),
                $"The production composition should create exactly one live {typeof(T).Name} adapter.");
            return adapters[0];
        }

        private static Button RequireButton(Component screen)
        {
            Button button = screen.GetComponentsInChildren<Button>(true).FirstOrDefault();
            Assert.That(
                button,
                Is.Not.Null,
                $"The production {screen.GetType().Name} screen should provide the control this journey clicks.");
            return button;
        }

        private static string[] VisibleTexts(Component screen)
        {
            return screen.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text.isActiveAndEnabled)
                .Select(text => text.text)
                .ToArray();
        }

        private static Vector2 ScreenCenterOf(Button button)
        {
            RectTransform rectTransform = (RectTransform)button.transform;
            Canvas canvas = button.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            return RectTransformUtility.WorldToScreenPoint(eventCamera, rectTransform.TransformPoint(rectTransform.rect.center));
        }
    }
}
#endif
