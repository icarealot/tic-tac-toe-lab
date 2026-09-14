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
        private GameObject _root;
        private TestComponentFactory _factory;
        private BoardView _board;

        [SetUp]
        public void CreateIsolatedBoard()
        {
            _root = new GameObject("BoardViewTests");
            _factory = new TestComponentFactory();
            _board = _root.AddComponent<BoardView>();
            _board.Construct(_factory, 3, new[]
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
        }

        [TearDown]
        public void DestroyIsolatedBoard()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
            }
        }

        [UnityTest]
        public IEnumerator Clearing_the_board_preserves_every_cell_while_removing_every_mark()
        {
            Assert.That(_board.GetComponentsInChildren<CellView>(), Has.Length.EqualTo(9), "The fixture should hold a full 3x3 board graph.");

            _board.ShowMark(0, 0, Mark.X);
            _board.ShowMark(2, 2, Mark.O);
            Assert.That(_board.GetComponentsInChildren<MarkView>(), Has.Length.EqualTo(2), "The fixture should show representative marks.");

            _board.Clear();
            yield return null;

            Assert.That(_board.GetComponentsInChildren<CellView>(), Has.Length.EqualTo(9));
            Assert.That(_board.GetComponentsInChildren<MarkView>(), Is.Empty);
        }
    }
}
#endif
