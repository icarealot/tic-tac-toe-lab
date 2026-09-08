#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardViewTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator Constructing_a_board_through_its_role_places_an_interface_based_cell_beneath_it_for_every_cell_placement()
        {
            yield return IE_LoadScene();

            RecordingFactoryService factory = new(Object.FindFirstObjectByType<FactoryService>());
            BoardModel boardModel = new();
            IBoardView boardView = factory.Get<IBoardView>();
            BoardView board = (BoardView)boardView;

            boardView.Construct(factory, boardModel.Dimension, boardModel.GetCellPlacements());

            Assert.That(factory.GetRequests, Has.Count.EqualTo(1 + boardModel.Dimension * boardModel.Dimension));
            Assert.That(factory.GetRequests[0].Type, Is.EqualTo(typeof(IBoardView)));
            for (int requestIndex = 1; requestIndex < factory.GetRequests.Count; requestIndex++)
            {
                Assert.That(factory.GetRequests[requestIndex].Type, Is.EqualTo(typeof(ICellView)));
                Assert.That(factory.GetRequests[requestIndex].Parent, Is.SameAs(board.transform));
            }

            Assert.That(board.GetComponentsInChildren<CellView>().Length, Is.EqualTo(boardModel.Dimension * boardModel.Dimension));
            foreach (CellPlacement placement in boardModel.GetCellPlacements())
            {
                Transform cellTransform = board.transform.Find($"Cell ({placement.Row}, {placement.Column})");
                Assert.That(cellTransform, Is.Not.Null, $"No cell was created for placement ({placement.Row}, {placement.Column}).");
                Assert.That(cellTransform.localPosition, Is.EqualTo(placement.LocalPoint));
            }

            factory.Return(boardView);
        }

        [UnityTest]
        public IEnumerator Clearing_the_board_view_leaves_every_cell_in_place_with_no_mark_showing()
        {
            yield return IE_LoadScene();

            BoardView boardView = Object.FindFirstObjectByType<BoardView>();
            boardView.ShowMark(0, 0, Mark.X);
            boardView.ShowMark(1, 1, Mark.O);
            int cellCountBeforeClear = boardView.GetComponentsInChildren<CellView>().Length;

            boardView.Clear();
            yield return null;

            Assert.That(boardView.GetComponentsInChildren<CellView>().Length, Is.EqualTo(cellCountBeforeClear));
            Assert.That(boardView.GetComponentsInChildren<MarkView>(), Is.Empty);
        }
    }
}
#endif
