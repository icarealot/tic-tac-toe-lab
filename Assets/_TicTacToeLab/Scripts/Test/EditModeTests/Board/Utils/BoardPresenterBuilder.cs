using TicTacToeLab.Runtime;

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
                new FakeCameraService(),
                new FakeLogService());
        }
    }
}
