using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardModelTests
    {
        [Test]
        public void The_local_point_of_each_cell_resolves_back_to_that_cells_own_coordinate()
        {
            BoardModel boardModel = new();

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    Vector3 center = boardModel.GetCellLocalPoint(row, column);

                    bool resolved = boardModel.TryResolveCell(center, out int resolvedRow, out int resolvedColumn);

                    Assert.That(resolved, Is.True);
                    Assert.That(resolvedRow, Is.EqualTo(row));
                    Assert.That(resolvedColumn, Is.EqualTo(column));
                }
            }
        }

        [Test]
        public void The_center_cells_center_point_is_the_origin()
        {
            BoardModel boardModel = new();

            Vector3 center = boardModel.GetCellLocalPoint(1, 1);

            Assert.That(center, Is.EqualTo(new Vector3(0f, 0f, 0f)));
        }

        [Test]
        public void The_top_right_cells_center_is_up_and_to_the_right_of_the_origin()
        {
            BoardModel boardModel = new();

            Vector3 center = boardModel.GetCellLocalPoint(0, 2);

            Assert.That(center, Is.EqualTo(new Vector3(1.2f, 1.2f, 0f)));
        }

        [Test]
        public void The_bottom_left_cells_center_is_down_and_to_the_left_of_the_origin()
        {
            BoardModel boardModel = new();

            Vector3 center = boardModel.GetCellLocalPoint(2, 0);

            Assert.That(center, Is.EqualTo(new Vector3(-1.2f, -1.2f, 0f)));
        }

        [Test]
        public void A_point_in_the_spacing_between_two_cells_resolves_to_no_cell()
        {
            BoardModel boardModel = new();

            bool resolved = boardModel.TryResolveCell(new Vector3(0.55f, 0f, 0f), out _, out _);

            Assert.That(resolved, Is.False);
        }

        [Test]
        public void A_point_beyond_the_boards_outer_edge_resolves_to_no_cell()
        {
            BoardModel boardModel = new();

            bool resolved = boardModel.TryResolveCell(new Vector3(10f, 10f, 0f), out _, out _);

            Assert.That(resolved, Is.False);
        }

        [Test]
        public void A_point_exactly_half_a_cell_size_from_a_center_resolves_to_that_cell()
        {
            BoardModel boardModel = new();

            Vector3 center = boardModel.GetCellLocalPoint(1, 1);

            bool resolved = boardModel.TryResolveCell(center + new Vector3(0.5f, 0.5f, 0f), out int row, out int column);

            Assert.That(resolved, Is.True);
            Assert.That(row, Is.EqualTo(1));
            Assert.That(column, Is.EqualTo(1));
        }

        [Test]
        public void A_point_just_beyond_half_a_cell_size_from_a_center_resolves_to_no_cell()
        {
            BoardModel boardModel = new();

            Vector3 center = boardModel.GetCellLocalPoint(1, 1);

            bool resolved = boardModel.TryResolveCell(center + new Vector3(0.51f, 0.51f, 0f), out _, out _);

            Assert.That(resolved, Is.False);
        }

        [Test]
        public void The_model_delivers_exactly_one_placement_per_cell_coordinate_at_that_cells_local_point()
        {
            BoardModel boardModel = new();

            IReadOnlyList<CellPlacement> cellPlacements = boardModel.GetCellPlacements();

            Assert.That(cellPlacements, Has.Count.EqualTo(9));

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    bool found = false;
                    foreach (CellPlacement placement in cellPlacements)
                    {
                        if (placement.Row != row || placement.Column != column)
                        {
                            continue;
                        }

                        found = true;
                        Assert.That(placement.LocalPoint, Is.EqualTo(boardModel.GetCellLocalPoint(row, column)));
                    }

                    Assert.That(found, Is.True, $"No placement was delivered for cell ({row}, {column}).");
                }
            }
        }

        [Test]
        public void A_fresh_board_has_X_to_play()
        {
            BoardModel boardModel = new();

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
        }

        [Test]
        public void A_fresh_board_is_in_progress()
        {
            BoardModel boardModel = new();

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void A_completed_row_is_a_win()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(1, 0); // O
            boardModel.PlaceMark(0, 1); // X
            boardModel.PlaceMark(1, 1); // O
            boardModel.PlaceMark(0, 2); // X completes row 0

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        [Test]
        public void A_completed_column_is_a_win()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(0, 1); // O
            boardModel.PlaceMark(1, 0); // X
            boardModel.PlaceMark(1, 1); // O
            boardModel.PlaceMark(2, 0); // X completes column 0

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        [Test]
        public void The_top_left_to_bottom_right_diagonal_is_a_win()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(0, 1); // O
            boardModel.PlaceMark(1, 1); // X
            boardModel.PlaceMark(0, 2); // O
            boardModel.PlaceMark(2, 2); // X completes the top-left to bottom-right diagonal

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        [Test]
        public void The_top_right_to_bottom_left_diagonal_is_a_win()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 2); // X
            boardModel.PlaceMark(0, 0); // O
            boardModel.PlaceMark(1, 1); // X
            boardModel.PlaceMark(0, 1); // O
            boardModel.PlaceMark(2, 0); // X completes the top-right to bottom-left diagonal

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        [Test]
        public void Two_of_three_in_a_line_is_still_in_progress()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(1, 0); // O
            boardModel.PlaceMark(0, 1); // X

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void A_line_holding_both_marks_is_not_a_win()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(0, 1); // O
            boardModel.PlaceMark(1, 0); // X
            boardModel.PlaceMark(1, 1); // O
            boardModel.PlaceMark(2, 1); // X
            boardModel.PlaceMark(0, 2); // O completes row 0 as X, O, O — mixed, not a win

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.InProgress));
        }

        [Test]
        public void The_turn_does_not_advance_on_a_win_so_the_turn_names_the_winner()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(1, 0); // O
            boardModel.PlaceMark(0, 1); // X
            boardModel.PlaceMark(1, 1); // O
            boardModel.PlaceMark(0, 2); // X completes row 0

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
        }

        [Test]
        public void Every_cell_reports_itself_empty_before_anything_is_placed()
        {
            BoardModel boardModel = new();

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    Assert.That(boardModel.IsEmpty(row, column), Is.True);
                }
            }
        }

        [Test]
        public void A_cell_reports_the_mark_it_was_given_and_no_other_cell_changes()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(1, 1);

            Assert.That(boardModel.GetMark(1, 1), Is.EqualTo(Mark.X));
            Assert.That(boardModel.IsEmpty(1, 1), Is.False);

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    if (row == 1 && column == 1)
                    {
                        continue;
                    }

                    Assert.That(boardModel.IsEmpty(row, column), Is.True);
                }
            }
        }

        [Test]
        public void Placing_a_mark_advances_the_turn_to_the_other_mark()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0);

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.O));
        }

        [Test]
        public void A_second_placement_advances_the_turn_back_to_X()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0);
            boardModel.PlaceMark(0, 1);

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
        }

        [Test]
        public void A_full_board_with_no_line_completed_is_a_draw()
        {
            BoardModel boardModel = new();

            BoardMoves.FillToDraw(boardModel);

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Draw));
        }

        [Test]
        public void The_turn_does_not_advance_on_a_draw()
        {
            BoardModel boardModel = new();

            BoardMoves.FillToDraw(boardModel);

            Assert.That(boardModel.Turn, Is.EqualTo(Mark.X));
        }

        [Test]
        public void A_win_completed_on_the_last_empty_cell_is_a_win_not_a_draw()
        {
            BoardModel boardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(2, 0); // O
            boardModel.PlaceMark(0, 1); // X
            boardModel.PlaceMark(1, 1); // O
            boardModel.PlaceMark(1, 0); // X
            boardModel.PlaceMark(1, 2); // O
            boardModel.PlaceMark(2, 1); // X
            boardModel.PlaceMark(2, 2); // O
            boardModel.PlaceMark(0, 2); // X completes row 0 and fills the last empty cell

            Assert.That(boardModel.Outcome, Is.EqualTo(Outcome.Win));
        }

        [Test]
        public void A_reset_board_is_indistinguishable_from_a_newly_constructed_one()
        {
            BoardModel boardModel = new();
            BoardModel freshBoardModel = new();

            boardModel.PlaceMark(0, 0); // X
            boardModel.PlaceMark(1, 0); // O
            boardModel.PlaceMark(0, 1); // X
            boardModel.PlaceMark(1, 1); // O
            boardModel.PlaceMark(0, 2); // X completes row 0

            boardModel.Reset();

            Assert.That(boardModel.Turn, Is.EqualTo(freshBoardModel.Turn));
            Assert.That(boardModel.Outcome, Is.EqualTo(freshBoardModel.Outcome));

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    Assert.That(boardModel.GetMark(row, column), Is.EqualTo(freshBoardModel.GetMark(row, column)));
                }
            }
        }
    }
}
