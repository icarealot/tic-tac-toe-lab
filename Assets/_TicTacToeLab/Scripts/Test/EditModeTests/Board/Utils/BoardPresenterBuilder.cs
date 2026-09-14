using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public static class BoardPresenterBuilder
    {
        private sealed class StubCameraService : ICameraService
        {
            public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
            {
                return (Vector3)screenPoint;
            }
        }

        private sealed class StubLogService : ILogService
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

        public static BoardPresenter Build(
            BoardModel boardModel,
            FakeBoardView boardView,
            FakeInputService inputService)
        {
            return new BoardPresenter(
                boardModel,
                boardView,
                inputService,
                new StubCameraService(),
                new StubLogService());
        }
    }
}
