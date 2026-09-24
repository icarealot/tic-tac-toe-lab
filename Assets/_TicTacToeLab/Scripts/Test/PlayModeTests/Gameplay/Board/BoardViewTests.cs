#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardViewTests
    {
        private GeneratedBoardFixture _fixture;

        [SetUp]
        public void CreateGeneratedBoard()
        {
            _fixture = new GeneratedBoardFixture();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedBoard()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Constructing_the_board_creates_one_cell_at_every_layout_placement()
        {
            // Arrange
            BoardLayout layout = _fixture.Layout;
            IReadOnlyList<CellPlacement> placements = layout.GetCellPlacements();

            // Act
            BoardView sut = _fixture.CreateBoard();

            // Assert
            CellView[] cells = sut.GetComponentsInChildren<CellView>();
            Assert.That(cells, Has.Length.EqualTo(placements.Count), "The board should create exactly one cell for every layout placement.");

            List<CellView> addressedCells = new(placements.Count);
            foreach (CellPlacement placement in placements)
            {
                CellView cell = _fixture.FindCell(sut, placement.Coordinate);
                Transform cellTransform = cell.transform;

                Assert.That(
                    cellTransform.localPosition,
                    Is.EqualTo(placement.LocalPoint),
                    $"The cell at ({placement.Coordinate.Row}, {placement.Coordinate.Column}) should sit at its board-local point.");
                addressedCells.Add(cell);
            }

            Assert.That(addressedCells, Is.Unique, "Every layout placement should address a distinct cell.");
        }

        [Test]
        public void Showing_a_mark_marks_only_the_addressed_cells()
        {
            // Arrange
            BoardView sut = _fixture.CreateBoard();
            BoardLayout layout = _fixture.Layout;
            IReadOnlyList<CellPlacement> placements = layout.GetCellPlacements();
            CellCoordinate[] markedCoordinates = { new(0, 1), new(2, 0) };

            // Act
            foreach (CellCoordinate coordinate in markedCoordinates)
            {
                sut.ShowMark(coordinate, Mark.X);
            }

            // Assert
            foreach (CellPlacement placement in placements)
            {
                CellView cell = _fixture.FindCell(sut, placement.Coordinate);
                MarkView[] liveMarks = cell.GetComponentsInChildren<MarkView>();
                bool wasAddressed = markedCoordinates.Contains(placement.Coordinate);

                Assert.That(
                    liveMarks,
                    Has.Length.EqualTo(wasAddressed ? 1 : 0),
                    $"The cell at ({placement.Coordinate.Row}, {placement.Coordinate.Column}) should hold exactly one mark only when it was addressed.");
            }
        }

        [UnityTest]
        public IEnumerator Clearing_the_board_destroys_every_owned_mark_and_preserves_its_cells()
        {
            // Arrange
            BoardView sut = _fixture.CreateBoard();
            CellCoordinate[] markedCoordinates = { new(0, 1), new(2, 0) };
            MarkView[] ownedMarks = new MarkView[markedCoordinates.Length];
            for (int index = 0; index < markedCoordinates.Length; index++)
            {
                CellCoordinate markedCoordinate = markedCoordinates[index];
                sut.ShowMark(markedCoordinate, index == 0 ? Mark.X : Mark.O);

                CellView markedCell = _fixture.FindCell(sut, markedCoordinate);
                ownedMarks[index] = markedCell.GetComponentInChildren<MarkView>();
            }

            CellView[] cells = sut.GetComponentsInChildren<CellView>();
            Assert.That(ownedMarks.All(mark => mark != null), Is.True, "The board should own a mark in every addressed cell.");

            // Act
            sut.Clear();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ownedMarks.All(mark => mark == null),
                "Clearing the board should destroy every mark the board owns.");
            Assert.That(sut.GetComponentsInChildren<MarkView>(), Is.Empty, "Clearing the board should leave no live mark on the board.");
            Assert.That(cells.All(cell => cell != null), Is.True, "Clearing the board should preserve every cell it owns.");
        }
    }
}
#endif
