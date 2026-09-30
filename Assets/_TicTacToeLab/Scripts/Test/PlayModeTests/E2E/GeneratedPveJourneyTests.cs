#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedPveJourneyTests
    {
        private GeneratedAppUIFixture _fixture;
        private AppUI _appUI;
        private BoardPresenter _boardPresenter;
        private BoardLayout _boardLayout;
        private RecordingBoardView _boardView;
        private JourneyInputService _inputService;
        private JourneyDelayScheduler _delayScheduler;
        private AppStateMachine _sut;

        [SetUp]
        public void CreateGeneratedPveComposition()
        {
            _fixture = new GeneratedAppUIFixture();
            _appUI = _fixture.CreateAppUI();
            _boardLayout = new BoardLayout(3);
            _boardView = new RecordingBoardView();
            _inputService = new JourneyInputService();
            _delayScheduler = new JourneyDelayScheduler();
            BoardModel boardModel = new();
            _boardPresenter = new BoardPresenter(
                boardModel,
                _boardLayout,
                _boardView,
                _inputService,
                new JourneyCameraService());
            _sut = new AppStateMachine(
                _boardPresenter,
                _appUI,
                _delayScheduler,
                _inputService,
                new FirstChoiceRandomService());
            _sut.Start();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedPveComposition()
        {
            _sut.Dispose();
            _boardPresenter.Dispose();
            yield return _fixture.IE_DestroyAll();
        }

        [UnityTest]
        public IEnumerator Selecting_Amateur_playing_to_a_bot_win_and_continuing_returns_Home()
        {
            // Arrange
            HomeScreen home = RequireActiveScreen<HomeScreen>();
            RequireButton(home, "PvE").onClick.Invoke();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ActiveScreen<BotSelectionScreen>() != null,
                "Selecting PvE should show the generated difficulty-selection screen.");
            BotSelectionScreen botSelection = RequireActiveScreen<BotSelectionScreen>();

            // Act
            RequireButton(botSelection, "Amateur").onClick.Invoke();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ActiveScreen<GameplayScreen>() != null,
                "Selecting a difficulty should show the generated gameplay screen.");
            GameplayScreen gameplay = RequireActiveScreen<GameplayScreen>();
            Assert.That(VisibleText(gameplay), Is.EqualTo("Your turn (X)"));

            // Act & assert: X and O alternate through observable marks and text.
            Press(new CellCoordinate(2, 2));
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _boardView.Marks.Count == 1 && ReadVisibleText(gameplay) == "Bot is thinking… (O)",
                "The first human placement should be observable with the bot-turn message.");
            Assert.That(_boardView.Marks[^1], Is.EqualTo((new CellCoordinate(2, 2), Mark.X)));

            _delayScheduler.FirePending();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _boardView.Marks.Count == 2 && ReadVisibleText(gameplay) == "Your turn (X)",
                "The first bot placement should restore the human turn observably.");
            Assert.That(_boardView.Marks[^1].Mark, Is.EqualTo(Mark.O));

            Press(new CellCoordinate(1, 1));
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ReadVisibleText(gameplay) == "Bot is thinking… (O)",
                "The second human placement should show the bot-turn message.");
            _delayScheduler.FirePending();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _boardView.Marks.Count == 4 && ReadVisibleText(gameplay) == "Your turn (X)",
                "The second bot placement should restore the human turn observably.");
            Assert.That(_boardView.Marks[^1].Mark, Is.EqualTo(Mark.O));

            Press(new CellCoordinate(1, 0));
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ReadVisibleText(gameplay) == "Bot is thinking… (O)",
                "The terminal human setup should show the bot-turn message before bot work runs.");

            // Act: finish the bot turn, then finish the existing delayed outcome presentation interval.
            _delayScheduler.FirePending();
            Assert.That(ActiveScreen<OutcomeScreen>(), Is.Null);
            _delayScheduler.FirePending();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ActiveScreen<OutcomeScreen>() != null,
                "A terminal bot placement should eventually show the generated outcome screen.");

            // Assert
            OutcomeScreen outcome = RequireActiveScreen<OutcomeScreen>();
            Assert.That(VisibleText(outcome), Is.EqualTo("Bot Wins!"));

            // Act
            RequireButton(outcome, "Continue").onClick.Invoke();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ActiveScreen<HomeScreen>() != null
                    && ActiveScreen<GameplayScreen>() == null
                    && ActiveScreen<OutcomeScreen>() == null,
                "Acknowledging the generated PvE outcome should return Home.");
            Assert.That(RequireActiveScreen<HomeScreen>(), Is.Not.Null);
        }

        private void Press(CellCoordinate coordinate)
        {
            Vector3 localPoint = _boardLayout.GetCellLocalPoint(coordinate);
            _inputService.RaisePress(new Vector2(localPoint.x, localPoint.y));
        }

        private T RequireActiveScreen<T>() where T : Component
        {
            T screen = ActiveScreen<T>();
            Assert.That(screen, Is.Not.Null, $"The generated journey should show an active {typeof(T).Name}.");
            return screen;
        }

        private T ActiveScreen<T>() where T : Component
        {
            return _appUI.GetComponentsInChildren<T>(true)
                .SingleOrDefault(screen => screen != null && screen.gameObject.activeInHierarchy);
        }

        private static Button RequireButton(Component screen, string nameFragment)
        {
            Button button = screen.GetComponentsInChildren<Button>(true)
                .SingleOrDefault(candidate => candidate.name.IndexOf(nameFragment, StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.That(button, Is.Not.Null, $"The generated {screen.GetType().Name} should provide a button containing '{nameFragment}'.");
            return button;
        }

        private static string VisibleText(Component screen)
        {
            string text = ReadVisibleText(screen);
            Assert.That(text, Is.Not.Null, $"The generated {screen.GetType().Name} should have one visible text value for this journey assertion.");
            return text;
        }

        private static string ReadVisibleText(Component screen)
        {
            if (screen == null)
            {
                return null;
            }

            TMP_Text[] texts = screen.GetComponentsInChildren<TMP_Text>(true)
                .Where(text => text.isActiveAndEnabled)
                .ToArray();
            return texts.Length == 1 ? texts[0].text : null;
        }

        private sealed class RecordingBoardView : IBoardView
        {
            public IReadOnlyList<(CellCoordinate Coordinate, Mark Mark)> Marks => _marks;

            private readonly List<(CellCoordinate Coordinate, Mark Mark)> _marks = new();

            public Vector3 ToLocalPoint(Vector3 worldPoint)
            {
                return worldPoint;
            }

            public void ShowMark(CellCoordinate coordinate, Mark mark)
            {
                _marks.Add((coordinate, mark));
            }

            public void Clear()
            {
                _marks.Clear();
            }
        }

        private sealed class JourneyInputService : IInputService
        {
            public event Action<Vector2> Pressed;
            public event Action BackPressed;

            private bool _isPlayerPressEnabled;

            public void RaisePress(Vector2 screenPoint)
            {
                if (_isPlayerPressEnabled)
                {
                    Pressed?.Invoke(screenPoint);
                }
            }

            public void EnablePlayerPress()
            {
                _isPlayerPressEnabled = true;
            }

            public void DisablePlayerPress()
            {
                _isPlayerPressEnabled = false;
            }
        }

        private sealed class JourneyCameraService : ICameraService
        {
            public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
            {
                return new Vector3(screenPoint.x, screenPoint.y, 0f);
            }
        }

        private sealed class JourneyDelayScheduler : IDelayScheduler
        {
            private Action _pendingCallback;

            public IDisposable Schedule(float delaySeconds, Action callback)
            {
                _pendingCallback = callback;
                return new Cancellation(this, callback);
            }

            public void FirePending()
            {
                Action callback = _pendingCallback;
                _pendingCallback = null;
                Assert.That(callback, Is.Not.Null, "The generated journey should have observable pending work before it is fired.");
                callback();
            }

            private void Cancel(Action callback)
            {
                if (ReferenceEquals(_pendingCallback, callback))
                {
                    _pendingCallback = null;
                }
            }

            private sealed class Cancellation : IDisposable
            {
                private JourneyDelayScheduler _scheduler;
                private readonly Action _callback;

                public Cancellation(JourneyDelayScheduler scheduler, Action callback)
                {
                    _scheduler = scheduler;
                    _callback = callback;
                }

                public void Dispose()
                {
                    if (_scheduler != null)
                    {
                        _scheduler.Cancel(_callback);
                        _scheduler = null;
                    }
                }
            }
        }

        private sealed class FirstChoiceRandomService : IRandomService
        {
            public int Range(int minimumInclusive, int maximumExclusive)
            {
                return minimumInclusive;
            }

            public float Range(float minimumInclusive, float maximumInclusive)
            {
                return minimumInclusive;
            }
        }
    }
}
#endif
