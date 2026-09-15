using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardSessionTests
    {
        [Test]
        public void A_reset_requested_through_the_session_leaves_the_game_fresh()
        {
            // Arrange
            BoardModel boardModel = new();

            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            BoardPresser presser = new(fakeInputService, boardModel);

            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(boardModel, fakeBoardView, fakeInputService);
            BoardSession sut = new(boardPresenter);

            BoardMoves.WinRowZeroForX(presser);

            // Act
            sut.Reset();

            // Assert
            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }
    }
}
