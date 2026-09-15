using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardSessionTests
    {
        [Test]
        public void A_board_session_reports_an_in_progress_outcome_for_a_fresh_game()
        {
            // Arrange
            BoardModel boardModel = new();
            FakeInputService fakeInputService = new();
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(boardModel, new FakeBoardView(), fakeInputService);
            BoardSession sut = new(boardPresenter);

            // Assert
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        private static IEnumerable<TestCaseData> WonGames()
        {
            yield return new TestCaseData((Action<BoardPresser>)BoardMoves.WinRowZeroForX, Mark.X)
                .SetName("A_board_session_reports_a_win_for_X_with_the_frozen_turn_as_the_winning_mark");

            yield return new TestCaseData((Action<BoardPresser>)BoardMoves.WinRowZeroForO, Mark.O)
                .SetName("A_board_session_reports_a_win_for_O_with_the_frozen_turn_as_the_winning_mark");
        }

        [TestCaseSource(nameof(WonGames))]
        public void A_board_session_reports_a_win_after_a_won_line_is_completed(Action<BoardPresser> completeWin, Mark winningMark)
        {
            // Arrange
            BoardModel boardModel = new();
            FakeInputService fakeInputService = new();
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(boardModel, new FakeBoardView(), fakeInputService);
            BoardSession sut = new(boardPresenter);
            BoardPresser presser = new(fakeInputService, boardModel);

            // Act
            completeWin(presser);

            // Assert
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.Win));
            Assert.That(sut.Turn, Is.EqualTo(winningMark), "The frozen turn names the winner.");
        }

        [Test]
        public void A_board_session_reports_a_draw_after_the_board_fills_without_a_won_line()
        {
            // Arrange
            BoardModel boardModel = new();
            FakeInputService fakeInputService = new();
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(boardModel, new FakeBoardView(), fakeInputService);
            BoardSession sut = new(boardPresenter);
            BoardPresser presser = new(fakeInputService, boardModel);

            // Act
            BoardMoves.FillForDraw(presser);

            // Assert
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.Draw));
        }

        [Test]
        public void A_reset_requested_through_the_session_returns_the_exposed_outcome_to_in_progress_and_the_exposed_turn_to_X()
        {
            // Arrange
            BoardModel boardModel = new();
            FakeInputService fakeInputService = new();
            BoardPresenter boardPresenter = BoardPresenterBuilder.Build(boardModel, new FakeBoardView(), fakeInputService);
            BoardSession sut = new(boardPresenter);
            BoardPresser presser = new(fakeInputService, boardModel);

            BoardMoves.WinRowZeroForO(presser);

            // Act
            sut.Reset();

            // Assert
            Assert.That(sut.Outcome, Is.EqualTo(Outcome.InProgress));
            Assert.That(sut.Turn, Is.EqualTo(Mark.X));
        }
    }
}
