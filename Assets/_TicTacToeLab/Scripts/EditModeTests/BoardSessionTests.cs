using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardSessionTests
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

        private BoardSession CreateSession(
            BoardModel boardModel,
            FakeBoardView boardView,
            FakeInputService inputService)
        {
            BoardPresenter boardPresenter = new(
                boardModel, boardView, factoryService: null,
                inputService, new FakeCameraService(), new FakeLogService());
            return new BoardSession(boardPresenter);
        }

        private void WinRowZeroForX(FakeInputService fakeInputService, BoardModel boardModel)
        {
            PressCell(fakeInputService, boardModel, 0, 0); // X
            PressCell(fakeInputService, boardModel, 1, 0); // O
            PressCell(fakeInputService, boardModel, 0, 1); // X
            PressCell(fakeInputService, boardModel, 1, 1); // O
            PressCell(fakeInputService, boardModel, 0, 2); // X completes row 0
        }

        private void PressCell(FakeInputService fakeInputService, BoardModel boardModel, int row, int column)
        {
            Vector3 cellCenter = boardModel.GetCellLocalPoint(row, column);
            fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));
        }

        [Test]
        public void The_session_raises_its_ending_event_when_the_presenter_ends_the_game()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            int gameEndedCount = 0;
            boardSession.GameEnded += () => gameEndedCount++;

            WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(1));
        }

        [Test]
        public void Resetting_the_game_through_the_session_reaches_the_presenter_and_clears_the_view()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            WinRowZeroForX(fakeInputService, boardModel);

            boardSession.Reset();

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(fakeBoardView.WasCleared, Is.True);
        }

        [Test]
        public void Disposing_the_session_unsubscribes_from_and_disposes_the_presenter_so_a_later_ending_raises_nothing()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardSession boardSession = CreateSession(boardModel, fakeBoardView, fakeInputService);
            int gameEndedCount = 0;
            boardSession.GameEnded += () => gameEndedCount++;

            boardSession.Dispose();
            WinRowZeroForX(fakeInputService, boardModel);

            Assert.That(gameEndedCount, Is.EqualTo(0));
            Assert.That(fakeInputService.HasSubscribers, Is.False);
        }
    }
}
