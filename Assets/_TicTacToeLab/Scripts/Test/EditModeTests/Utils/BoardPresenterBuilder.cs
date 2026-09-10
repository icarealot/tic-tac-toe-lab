using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    /// <summary>
    /// The one construction path shared by the board presenter and board session tests: both wire
    /// the same presenter over the same fakes, so the collaborator wiring lives here rather than
    /// being restated in each test class.
    /// </summary>
    public static class BoardPresenterBuilder
    {
        public static BoardPresenter Build(
            BoardModel boardModel,
            FakeBoardView boardView,
            FakeInputService inputService,
            FakeLogService logService)
        {
            return new BoardPresenter(
                boardModel, boardView,
                inputService, new FakeCameraService(), logService);
        }
    }
}
