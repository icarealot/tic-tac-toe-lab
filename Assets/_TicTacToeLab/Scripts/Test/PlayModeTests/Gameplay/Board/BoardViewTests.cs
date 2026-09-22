#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardViewTests
    {
        private const int BOARD_DIMENSION = 3;

        private GameObject _root;
        private GameObject _cellTemplate;
        private GameObject _markTemplate;
        private BoardLayout _boardLayout;
        private BoardView _sut;

        [SetUp]
        public void CreateIsolatedBoard()
        {
            _root = new GameObject("BoardViewTests");
            _markTemplate = new GameObject("MarkTemplate");
            MarkView markPrefab = _markTemplate.AddComponent<MarkView>();

            _cellTemplate = new GameObject("CellTemplate");
            CellView cellPrefab = _cellTemplate.AddComponent<CellView>();
            TestSerializedReference.AssignPrefab(cellPrefab, "_markViewPrefab", markPrefab);

            _boardLayout = new BoardLayout(BOARD_DIMENSION);
            _sut = _root.AddComponent<BoardView>();
            TestSerializedReference.AssignPrefab(_sut, "_cellViewPrefab", cellPrefab);
        }

        [UnityTearDown]
        public IEnumerator DestroyIsolatedBoard()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
            }

            if (_cellTemplate != null)
            {
                Object.Destroy(_cellTemplate);
            }

            if (_markTemplate != null)
            {
                Object.Destroy(_markTemplate);
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _root == null && _cellTemplate == null && _markTemplate == null,
                "The board fixture should be destroyed after the test.");
        }

        [Test]
        public void Constructing_the_board_creates_every_cell_at_its_layout_placement()
        {
            // Arrange
            IReadOnlyList<CellPlacement> placements = _boardLayout.GetCellPlacements();

            // Act
            _sut.Construct(BOARD_DIMENSION, placements);

            // Assert
            CellView[] cells = _sut.GetComponentsInChildren<CellView>();
            Assert.That(cells, Has.Length.EqualTo(placements.Count), "The board should create one cell for every layout placement.");

            foreach (CellView cell in cells)
            {
                Assert.That(cell.transform.parent, Is.EqualTo(_sut.transform), "The board should parent every cell it creates.");
            }

            foreach (CellPlacement placement in placements)
            {
                Assert.That(CellAt(placement.Coordinate), Is.Not.Null, $"The board should create a cell at placement ({placement.Coordinate.Row}, {placement.Coordinate.Column}).");
            }
        }

        [Test]
        public void Showing_a_mark_places_it_under_the_cell_at_the_addressed_coordinate()
        {
            // Arrange
            _sut.Construct(BOARD_DIMENSION, _boardLayout.GetCellPlacements());

            // Act
            _sut.ShowMark(new CellCoordinate(0, 1), Mark.X);
            _sut.ShowMark(new CellCoordinate(2, 0), Mark.O);

            // Assert
            Assert.That(CellAt(new CellCoordinate(0, 1)).GetComponentInChildren<MarkView>(), Is.Not.Null, "The X should appear under the cell at coordinate (0, 1).");
            Assert.That(CellAt(new CellCoordinate(2, 0)).GetComponentInChildren<MarkView>(), Is.Not.Null, "The O should appear under the cell at coordinate (2, 0).");
            Assert.That(CellAt(new CellCoordinate(1, 1)).GetComponentInChildren<MarkView>(), Is.Null, "Only the addressed cells should hold a mark.");
            Assert.That(_sut.GetComponentsInChildren<MarkView>(), Has.Length.EqualTo(2), "The fixture should show representative marks.");
        }

        [UnityTest]
        public IEnumerator Clearing_the_board_destroys_every_mark_and_preserves_every_cell()
        {
            // Arrange
            _sut.Construct(BOARD_DIMENSION, _boardLayout.GetCellPlacements());
            CellView[] cellsBefore = _sut.GetComponentsInChildren<CellView>();
            _sut.ShowMark(new CellCoordinate(0, 1), Mark.X);
            _sut.ShowMark(new CellCoordinate(2, 0), Mark.O);
            Assert.That(_sut.GetComponentsInChildren<MarkView>(), Has.Length.EqualTo(2), "The fixture should show representative marks.");

            // Act
            _sut.Clear();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _sut.GetComponentsInChildren<MarkView>().Length == 0,
                "Clearing the board should remove every mark.");
            Assert.That(_sut.GetComponentsInChildren<CellView>(), Is.EquivalentTo(cellsBefore), "Clearing the board should preserve every cell instance.");
        }

        [Test]
        public void Clearing_an_empty_board_repeatedly_is_harmless()
        {
            // Arrange
            _sut.Construct(BOARD_DIMENSION, _boardLayout.GetCellPlacements());

            // Act
            TestDelegate clearEmptyBoard = () =>
            {
                _sut.Clear();
                _sut.Clear();
            };

            // Assert
            Assert.That(clearEmptyBoard, Throws.Nothing);
            Assert.That(_sut.GetComponentsInChildren<CellView>(), Has.Length.EqualTo(9), "Repeated clearing should preserve every cell.");
        }

        private CellView CellAt(CellCoordinate coordinate)
        {
            Vector3 localPoint = _boardLayout.GetCellLocalPoint(coordinate);

            foreach (CellView cell in _sut.GetComponentsInChildren<CellView>())
            {
                if (cell.transform.localPosition == localPoint)
                {
                    return cell;
                }
            }

            Assert.Fail($"The board view should create a cell at coordinate ({coordinate.Row}, {coordinate.Column}).");
            return null;
        }
    }
}
#endif
