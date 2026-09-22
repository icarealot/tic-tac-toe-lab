using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardLayoutTests
    {
        [Test]
        public void The_layout_produces_exactly_one_placement_for_every_cell()
        {
            BoardLayout sut = new(3);

            IReadOnlyList<CellPlacement> placements = sut.GetCellPlacements();

            HashSet<CellCoordinate> addressedCells = new();
            foreach (CellPlacement placement in placements)
            {
                _ = addressedCells.Add(placement.Coordinate);
            }

            CellCoordinate[] everyCell =
            {
                new(0, 0), new(0, 1), new(0, 2),
                new(1, 0), new(1, 1), new(1, 2),
                new(2, 0), new(2, 1), new(2, 2),
            };

            Assert.That(placements.Count, Is.EqualTo(9));
            Assert.That(addressedCells, Is.EquivalentTo(everyCell));
        }

        private static IEnumerable<TestCaseData> CellPlacements()
        {
            yield return new TestCaseData(new CellCoordinate(0, 0), new Vector3(-1.2f, 1.2f, 0f));
            yield return new TestCaseData(new CellCoordinate(0, 1), new Vector3(0f, 1.2f, 0f));
            yield return new TestCaseData(new CellCoordinate(0, 2), new Vector3(1.2f, 1.2f, 0f));
            yield return new TestCaseData(new CellCoordinate(1, 0), new Vector3(-1.2f, 0f, 0f));
            yield return new TestCaseData(new CellCoordinate(1, 1), new Vector3(0f, 0f, 0f));
            yield return new TestCaseData(new CellCoordinate(1, 2), new Vector3(1.2f, 0f, 0f));
            yield return new TestCaseData(new CellCoordinate(2, 0), new Vector3(-1.2f, -1.2f, 0f));
            yield return new TestCaseData(new CellCoordinate(2, 1), new Vector3(0f, -1.2f, 0f));
            yield return new TestCaseData(new CellCoordinate(2, 2), new Vector3(1.2f, -1.2f, 0f));
        }

        [TestCaseSource(nameof(CellPlacements))]
        public void A_cell_placement_centers_the_addressed_cell_from_the_top_left_origin(CellCoordinate coordinate, Vector3 expectedLocalPoint)
        {
            BoardLayout sut = new(3);

            CellPlacement placement = FindPlacement(sut, coordinate);

            Assert.That(placement.LocalPoint, Is.EqualTo(expectedLocalPoint), $"Cell ({coordinate.Row}, {coordinate.Column}) should be centered at {expectedLocalPoint}.");
        }

        private static IEnumerable<TestCaseData> CellCenters()
        {
            yield return new TestCaseData(new Vector3(-1.2f, 1.2f, 0f), new CellCoordinate(0, 0));
            yield return new TestCaseData(new Vector3(0f, 1.2f, 0f), new CellCoordinate(0, 1));
            yield return new TestCaseData(new Vector3(1.2f, 1.2f, 0f), new CellCoordinate(0, 2));
            yield return new TestCaseData(new Vector3(-1.2f, 0f, 0f), new CellCoordinate(1, 0));
            yield return new TestCaseData(new Vector3(0f, 0f, 0f), new CellCoordinate(1, 1));
            yield return new TestCaseData(new Vector3(1.2f, 0f, 0f), new CellCoordinate(1, 2));
            yield return new TestCaseData(new Vector3(-1.2f, -1.2f, 0f), new CellCoordinate(2, 0));
            yield return new TestCaseData(new Vector3(0f, -1.2f, 0f), new CellCoordinate(2, 1));
            yield return new TestCaseData(new Vector3(1.2f, -1.2f, 0f), new CellCoordinate(2, 2));
        }

        [TestCaseSource(nameof(CellCenters))]
        public void A_board_local_point_at_a_cell_center_resolves_to_the_addressed_cell(Vector3 boardLocalPoint, CellCoordinate expectedCoordinate)
        {
            BoardLayout sut = new(3);

            bool resolved = sut.TryResolvePoint(boardLocalPoint, out CellCoordinate coordinate);

            Assert.That(resolved, Is.True);
            Assert.That(coordinate, Is.EqualTo(expectedCoordinate));
        }

        private static IEnumerable<TestCaseData> CellEdgePoints()
        {
            yield return new TestCaseData(new Vector3(-1.7f, 1.7f, 0f), new CellCoordinate(0, 0));
            yield return new TestCaseData(new Vector3(1.7f, 1.7f, 0f), new CellCoordinate(0, 2));
            yield return new TestCaseData(new Vector3(-1.7f, -1.7f, 0f), new CellCoordinate(2, 0));
            yield return new TestCaseData(new Vector3(1.7f, -1.7f, 0f), new CellCoordinate(2, 2));
            yield return new TestCaseData(new Vector3(-0.5f, 1.2f, 0f), new CellCoordinate(0, 1));
            yield return new TestCaseData(new Vector3(0.5f, 1.2f, 0f), new CellCoordinate(0, 1));
            yield return new TestCaseData(new Vector3(-1.2f, -0.5f, 0f), new CellCoordinate(1, 0));
            yield return new TestCaseData(new Vector3(-1.2f, 0.5f, 0f), new CellCoordinate(1, 0));
        }

        [TestCaseSource(nameof(CellEdgePoints))]
        public void A_board_local_point_on_a_cell_edge_still_resolves_to_the_addressed_cell(Vector3 boardLocalPoint, CellCoordinate expectedCoordinate)
        {
            BoardLayout sut = new(3);

            bool resolved = sut.TryResolvePoint(boardLocalPoint, out CellCoordinate coordinate);

            Assert.That(resolved, Is.True);
            Assert.That(coordinate, Is.EqualTo(expectedCoordinate));
        }

        private static IEnumerable<TestCaseData> SpacingPoints()
        {
            yield return new TestCaseData(new Vector3(-0.6f, 1.2f, 0f));
            yield return new TestCaseData(new Vector3(0.6f, 1.2f, 0f));
            yield return new TestCaseData(new Vector3(-1.2f, 0.6f, 0f));
            yield return new TestCaseData(new Vector3(-1.2f, -0.6f, 0f));
        }

        [TestCaseSource(nameof(SpacingPoints))]
        public void A_board_local_point_inside_spacing_resolves_to_no_cell(Vector3 boardLocalPoint)
        {
            BoardLayout sut = new(3);

            bool resolved = sut.TryResolvePoint(boardLocalPoint, out _);

            Assert.That(resolved, Is.False);
        }

        private static IEnumerable<TestCaseData> OutsideBoardPoints()
        {
            yield return new TestCaseData(new Vector3(-1.8f, 1.2f, 0f));
            yield return new TestCaseData(new Vector3(1.8f, 1.2f, 0f));
            yield return new TestCaseData(new Vector3(-1.2f, 1.8f, 0f));
            yield return new TestCaseData(new Vector3(-1.2f, -1.8f, 0f));
            yield return new TestCaseData(new Vector3(-1.75f, 1.2f, 0f));
            yield return new TestCaseData(new Vector3(4f, 4f, 0f));
        }

        [TestCaseSource(nameof(OutsideBoardPoints))]
        public void A_board_local_point_beyond_the_board_resolves_to_no_cell(Vector3 boardLocalPoint)
        {
            BoardLayout sut = new(3);

            bool resolved = sut.TryResolvePoint(boardLocalPoint, out _);

            Assert.That(resolved, Is.False);
        }

        private static CellPlacement FindPlacement(BoardLayout layout, CellCoordinate coordinate)
        {
            CellPlacement foundPlacement = default;
            bool wasFound = false;

            foreach (CellPlacement placement in layout.GetCellPlacements())
            {
                if (placement.Coordinate == coordinate)
                {
                    foundPlacement = placement;
                    wasFound = true;
                }
            }

            Assert.That(wasFound, Is.True, $"The layout produced no placement for cell ({coordinate.Row}, {coordinate.Column}).");
            return foundPlacement;
        }
    }
}
