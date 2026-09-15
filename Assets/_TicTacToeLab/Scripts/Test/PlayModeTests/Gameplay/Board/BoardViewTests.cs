#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardViewTests
    {

        [UnityTest]
        public IEnumerator Clearing_the_board_preserves_every_cell_while_removing_every_mark()
        {
            // Arrange
            GameObject root = new("BoardViewTests");
            TestComponentFactory factory = new();
            BoardView sut = root.AddComponent<BoardView>();
            sut.Construct(factory, 3, new[]
            {
                new CellPlacement(0, 0, new Vector3(0f, 0f, 0f)),
                new CellPlacement(0, 1, new Vector3(1f, 0f, 0f)),
                new CellPlacement(0, 2, new Vector3(2f, 0f, 0f)),
                new CellPlacement(1, 0, new Vector3(0f, -1f, 0f)),
                new CellPlacement(1, 1, new Vector3(1f, -1f, 0f)),
                new CellPlacement(1, 2, new Vector3(2f, -1f, 0f)),
                new CellPlacement(2, 0, new Vector3(0f, -2f, 0f)),
                new CellPlacement(2, 1, new Vector3(1f, -2f, 0f)),
                new CellPlacement(2, 2, new Vector3(2f, -2f, 0f)),
            });

            sut.ShowMark(0, 0, Mark.X);
            sut.ShowMark(2, 2, Mark.O);
            Assert.That(sut.GetComponentsInChildren<MarkView>(), Has.Length.EqualTo(2), "The fixture should show representative marks.");

            // Act
            sut.Clear();
            yield return null;

            // Assert
            Assert.That(sut.GetComponentsInChildren<CellView>(), Has.Length.EqualTo(9));
            Assert.That(sut.GetComponentsInChildren<MarkView>(), Is.Empty);

            // Clean up
            yield return null;

            Object.Destroy(root);
        }
    }
}
#endif
