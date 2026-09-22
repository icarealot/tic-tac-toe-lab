using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class CellCoordinateTests
    {
        [Test]
        public void A_cell_coordinate_reports_row_before_column_as_written()
        {
            CellCoordinate coordinate = new(2, 1);

            Assert.That(coordinate.Row, Is.EqualTo(2));
            Assert.That(coordinate.Column, Is.EqualTo(1));
        }

        [Test]
        public void Cell_coordinates_with_the_same_row_and_column_are_equal()
        {
            CellCoordinate first = new(1, 2);
            CellCoordinate second = new(1, 2);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [TestCase(0, 0)]
        [TestCase(2, 2)]
        [TestCase(1, 2)]
        public void A_cell_coordinate_inside_the_board_dimension_is_within_it(int row, int column)
        {
            CellCoordinate coordinate = new(row, column);

            Assert.That(coordinate.IsWithin(3), Is.True);
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(3, 0)]
        [TestCase(0, 3)]
        [TestCase(3, 3)]
        public void A_cell_coordinate_outside_the_board_dimension_is_not_within_it(int row, int column)
        {
            CellCoordinate coordinate = new(row, column);

            Assert.That(coordinate.IsWithin(3), Is.False);
        }

        [Test]
        public void An_out_of_range_cell_coordinate_keeps_its_values_so_callers_must_check_within_before_use()
        {
            CellCoordinate coordinate = new(3, -1);

            Assert.That(coordinate.Row, Is.EqualTo(3));
            Assert.That(coordinate.Column, Is.EqualTo(-1));
        }

        [TestCase(2, 1, 1, 2)]
        [TestCase(1, 2, 0, 2)]
        [TestCase(1, 2, 1, 0)]
        public void Cell_coordinates_that_differ_in_row_or_column_are_not_equal(int firstRow, int firstColumn, int secondRow, int secondColumn)
        {
            CellCoordinate first = new(firstRow, firstColumn);
            CellCoordinate second = new(secondRow, secondColumn);

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first == second, Is.False);
            Assert.That(first != second, Is.True);
        }
    }
}
