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
                // A non-identity conversion proves the presenter applies the camera boundary.
                return new Vector3(screenPoint.x * 2f, screenPoint.y * 2f, 0f);
            }
        }

        public static BoardPresenter Build(
            BoardModel boardModel,
            BoardLayout boardLayout,
            FakeBoardView boardView,
            FakeInputService inputService)
        {
            return new BoardPresenter(
                boardModel,
                boardLayout,
                boardView,
                inputService,
                new StubCameraService());
        }
    }
}
