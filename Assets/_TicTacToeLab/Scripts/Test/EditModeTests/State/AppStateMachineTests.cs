using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class AppStateMachineTests
    {
        private abstract class SpyState : IAppState
        {
            protected readonly List<string> Log;

            protected SpyState(List<string> log)
            {
                Log = log;
            }

            public void Enter()
            {
                Log.Add($"{GetType().Name}.Enter");
            }

            public void Leave()
            {
                Log.Add($"{GetType().Name}.Leave");
            }
        }

        private sealed class FirstSpyState : SpyState
        {
            public FirstSpyState(List<string> log) : base(log)
            {
            }
        }

        private sealed class SecondSpyState : SpyState
        {
            public SecondSpyState(List<string> log) : base(log)
            {
            }
        }

        [Test]
        public void Changing_state_leaves_the_current_state_before_entering_the_next()
        {
            List<string> log = new();
            AppStateMachine stateMachine = new();
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.Add(new SecondSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            stateMachine.ChangeState<SecondSpyState>();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave", "SecondSpyState.Enter" }));
        }

        [Test]
        public void Disposing_the_machine_leaves_the_current_state()
        {
            List<string> log = new();
            AppStateMachine stateMachine = new();
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            stateMachine.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));
        }

        private sealed class FakeBoardView : IBoardView
        {
            public bool WasCleared { get; private set; }
            public int ClearCount { get; private set; }

            public void Construct(IFactoryService factoryService, int dimension, IReadOnlyList<CellPlacement> placements)
            {
            }

            public Vector3 ToLocalPoint(Vector3 worldPoint)
            {
                return worldPoint;
            }

            public void ShowMark(int row, int column, Mark mark)
            {
            }

            public void Clear()
            {
                WasCleared = true;
                ClearCount++;
            }
        }

        private sealed class FakeInputService : IInputService
        {
            public event Action<Vector2> Pressed;

            public bool HasSubscribers => Pressed != null;

            public void RaisePress(Vector2 screenPoint)
            {
                Pressed?.Invoke(screenPoint);
            }
        }

        private sealed class FakeCameraService : ICameraService
        {
            public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
            {
                return new Vector3(screenPoint.x, screenPoint.y, 0f);
            }
        }

        private sealed class FakeLogService : ILogService
        {
            public void Log(string message)
            {
            }

            public void LogWarning(string message)
            {
            }

            public void LogError(string message)
            {
            }
        }

        private BoardModel _boardModel;
        private FakeBoardView _fakeBoardView;
        private FakeInputService _fakeInputService;
        private BoardSession _boardSession;
        private FakeCoroutineService _fakeCoroutineService;
        private AppStateMachine _stateMachine;
        private GameplayState _gameplayState;
        private GameCompleteState _gameCompleteState;

        [SetUp]
        public void SetUp()
        {
            _boardModel = new BoardModel();
            _fakeBoardView = new FakeBoardView();
            _fakeInputService = new FakeInputService();
            BoardPresenter boardPresenter = new(
                _boardModel, _fakeBoardView, factoryService: null,
                _fakeInputService, new FakeCameraService(), new FakeLogService());
            _boardSession = new BoardSession(boardPresenter);
            _fakeCoroutineService = new FakeCoroutineService();
            _stateMachine = new AppStateMachine();
            _gameplayState = new GameplayState(_boardSession, _stateMachine);
            _gameCompleteState = new GameCompleteState(_boardSession, _stateMachine, _fakeCoroutineService);
            _stateMachine.Add(_gameplayState);
            _stateMachine.Add(_gameCompleteState);
        }

        private void PressCell(int row, int column)
        {
            Vector3 cellCenter = _boardModel.GetCellLocalPoint(row, column);
            _fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));
        }

        private void WinRowZeroForX()
        {
            PressCell(0, 0); // X
            PressCell(1, 0); // O
            PressCell(0, 1); // X
            PressCell(1, 1); // O
            PressCell(0, 2); // X completes row 0
        }

        [Test]
        public void The_gameplay_state_subscribes_to_the_session_ending_on_entry()
        {
            _gameplayState.Enter();

            WinRowZeroForX();
            _fakeCoroutineService.PumpToCompletion();

            Assert.That(_fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void The_gameplay_state_unsubscribes_from_the_session_ending_on_exit()
        {
            _gameplayState.Enter();
            _gameplayState.Leave();

            WinRowZeroForX();

            Assert.That(_fakeBoardView.WasCleared, Is.False);
            Assert.That(_boardModel.Outcome, Is.Not.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void The_gameplay_state_enters_the_game_complete_state_when_the_ending_is_announced()
        {
            _stateMachine.ChangeState<GameplayState>();

            WinRowZeroForX();
            _fakeCoroutineService.PumpToCompletion();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void The_game_complete_state_resets_the_game_before_it_returns_to_gameplay()
        {
            _stateMachine.ChangeState<GameCompleteState>();
            _fakeCoroutineService.PumpToCompletion();

            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(_fakeBoardView.WasCleared, Is.True);

            // Being back in gameplay is what lets a freshly-started game end on its own again.
            WinRowZeroForX();
            _fakeCoroutineService.PumpToCompletion();
            Assert.That(_fakeBoardView.WasCleared, Is.True);
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void Re_entering_gameplay_after_each_game_does_not_accumulate_subscriptions()
        {
            _stateMachine.ChangeState<GameplayState>();

            WinRowZeroForX();
            _fakeCoroutineService.PumpToCompletion();
            WinRowZeroForX();
            _fakeCoroutineService.PumpToCompletion();
            WinRowZeroForX();
            _fakeCoroutineService.PumpToCompletion();

            // An extra, leftover subscription from a prior game would clear the board more than once per ending.
            Assert.That(_fakeBoardView.ClearCount, Is.EqualTo(3));
        }

        [Test]
        public void Many_games_in_a_row_leave_exactly_one_active_subscription()
        {
            _stateMachine.ChangeState<GameplayState>();

            for (int i = 0; i < 5; i++)
            {
                WinRowZeroForX();
                _fakeCoroutineService.PumpToCompletion();
            }

            _boardSession.Dispose();

            Assert.That(_fakeInputService.HasSubscribers, Is.False);
        }

        [Test]
        public void Tearing_down_the_machine_leaves_gameplay_so_a_subsequent_ending_reaches_nothing()
        {
            _stateMachine.ChangeState<GameplayState>();

            _stateMachine.Dispose();
            WinRowZeroForX();

            Assert.That(_fakeBoardView.WasCleared, Is.False);
            Assert.That(_boardModel.Outcome, Is.Not.EqualTo(Outcome.InProgress));
        }
    }
}
