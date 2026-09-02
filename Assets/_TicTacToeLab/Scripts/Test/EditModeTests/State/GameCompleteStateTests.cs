using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class GameCompleteStateTests
    {
        private sealed class FakeBoardView : IBoardView
        {
            public bool WasCleared { get; private set; }

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
            }
        }

        private sealed class FakeInputService : IInputService
        {
            public event Action<Vector2> Pressed;

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
            GameplayState gameplayState = new(_boardSession, _stateMachine);
            _gameCompleteState = new GameCompleteState(_boardSession, _stateMachine, _fakeCoroutineService);
            _stateMachine.Add(gameplayState);
            _stateMachine.Add(_gameCompleteState);
        }

        private void PressCell(int row, int column)
        {
            Vector3 cellCenter = _boardModel.GetCellLocalPoint(row, column);
            _fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));
        }

        [Test]
        public void Entering_the_state_starts_a_routine_and_does_not_reset_yet()
        {
            _gameCompleteState.Enter();

            Assert.That(_fakeCoroutineService.HasCapturedRoutine, Is.True);
            Assert.That(_fakeBoardView.WasCleared, Is.False);
        }

        [Test]
        public void Pumping_the_routine_to_completion_resets_the_game_and_then_returns_to_gameplay()
        {
            _gameCompleteState.Enter();

            _fakeCoroutineService.PumpToCompletion();

            Assert.That(_fakeBoardView.WasCleared, Is.True);
            Assert.That(_boardModel.Outcome, Is.EqualTo(Outcome.InProgress));

            // Being back in gameplay is what lets a fresh press place a mark.
            PressCell(0, 0);
            Assert.That(_boardModel.Turn, Is.EqualTo(Mark.O));
        }

        [Test]
        public void Leaving_the_state_disposes_the_pending_handle()
        {
            _gameCompleteState.Enter();

            _gameCompleteState.Leave();

            Assert.That(_fakeCoroutineService.WasStopped, Is.True);
        }

        [Test]
        public void A_disposed_pending_reset_does_not_later_reset_the_game()
        {
            _gameCompleteState.Enter();
            _gameCompleteState.Leave();

            _fakeCoroutineService.PumpToCompletion();

            Assert.That(_fakeBoardView.WasCleared, Is.False);
        }
    }
}
