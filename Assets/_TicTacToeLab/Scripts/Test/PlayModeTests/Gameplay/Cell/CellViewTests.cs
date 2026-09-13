#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Cell-owned mark lifecycle on one isolated cell: ShowMark gives the cell a mark created
    /// through the factory seam, ClearMark returns and removes exactly the mark the cell owns, and
    /// clearing again — or clearing a cell that never showed a mark — is harmless. Diagnostic
    /// names, sprite artwork, and child layout are incidental here and unasserted.
    /// </summary>
    public sealed class CellViewTests
    {
        private GameObject _root;
        private TestComponentFactory _factory;
        private CellView _cell;

        [SetUp]
        public void CreateIsolatedCell()
        {
            _root = new GameObject("CellViewTests");
            _factory = new TestComponentFactory();
            _cell = _root.AddComponent<CellView>();
            _cell.Construct(_factory, new CellPlacement(0, 0, Vector3.zero));
        }

        [TearDown]
        public void DestroyIsolatedCell()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
            }
        }

        [UnityTest]
        public IEnumerator Clearing_a_marked_cell_returns_and_removes_exactly_the_mark_it_owns()
        {
            _cell.ShowMark(Mark.X);
            MarkView ownedMark = _cell.GetComponentInChildren<MarkView>();
            Assert.That(ownedMark, Is.Not.Null, "ShowMark should leave the cell owning a mark.");

            _cell.ClearMark();

            Assert.That(_factory.Returned, Has.Count.EqualTo(1));
            Assert.That(_factory.Returned[0], Is.SameAs(ownedMark));

            yield return null;

            Assert.That(_cell.GetComponentInChildren<MarkView>(), Is.Null);

            _cell.ClearMark();
            Assert.That(_factory.Returned, Has.Count.EqualTo(1), "A repeated clear has nothing left to return.");
        }

        [Test]
        public void Clearing_a_cell_that_never_showed_a_mark_is_harmless()
        {
            Assert.That(() => _cell.ClearMark(), Throws.Nothing);
            Assert.That(_factory.Returned, Is.Empty);
        }
    }
}
#endif
