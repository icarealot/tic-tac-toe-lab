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
