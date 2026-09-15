#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CellViewTests
    {
        private GameObject _root;
        private TestComponentFactory _factory;
        private CellView _sut;

        [SetUp]
        public void CreateIsolatedCell()
        {
            _root = new GameObject("CellViewTests");
            _factory = new TestComponentFactory();
            _sut = _root.AddComponent<CellView>();
            _sut.Construct(_factory, new CellPlacement(0, 0, Vector3.zero));
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
            // Arrange
            _sut.ShowMark(Mark.X);
            MarkView ownedMark = _sut.GetComponentInChildren<MarkView>();
            Assert.That(ownedMark, Is.Not.Null, "ShowMark should leave the cell owning a mark.");

            // Act
            _sut.ClearMark();

            // Assert
            Assert.That(_factory.Returned, Has.Count.EqualTo(1));
            Assert.That(_factory.Returned[0], Is.SameAs(ownedMark));

            yield return null;

            Assert.That(_sut.GetComponentInChildren<MarkView>(), Is.Null);
        }

        [Test]
        public void Clearing_a_cell_that_never_showed_a_mark_is_harmless()
        {
            Assert.That(() => _sut.ClearMark(), Throws.Nothing);
            Assert.That(_factory.Returned, Is.Empty);
        }
    }
}
#endif
