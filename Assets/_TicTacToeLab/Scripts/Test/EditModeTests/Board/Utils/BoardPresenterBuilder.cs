using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public static class BoardPresenterBuilder
    {
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

        private sealed class StubCameraService : ICameraService
        {
            public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
            {
                return new Vector3(screenPoint.x, screenPoint.y, 0f);
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
    }
}
