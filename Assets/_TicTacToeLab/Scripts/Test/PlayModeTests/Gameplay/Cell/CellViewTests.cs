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
        private GameObject _markTemplate;
        private CellView _sut;

        [SetUp]
        public void CreateIsolatedCell()
        {
            _root = new GameObject("CellViewTests");
            _markTemplate = new GameObject("MarkTemplate");
            MarkView markPrefab = _markTemplate.AddComponent<MarkView>();

            _sut = _root.AddComponent<CellView>();
            TestSerializedReference.AssignPrefab(_sut, "_markViewPrefab", markPrefab);
            _sut.Construct(new CellPlacement(new CellCoordinate(0, 0), Vector3.zero));
        }

        [UnityTearDown]
        public IEnumerator DestroyIsolatedCell()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
            }

            if (_markTemplate != null)
            {
                Object.Destroy(_markTemplate);
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _root == null && _markTemplate == null,
                "The cell fixture should be destroyed after the test.");
        }

        [Test]
        public void Showing_a_mark_creates_one_mark_parented_under_the_cell()
        {
            // Act
            _sut.ShowMark(Mark.X);

            // Assert
            MarkView[] marks = _sut.GetComponentsInChildren<MarkView>();
            Assert.That(marks, Has.Length.EqualTo(1), "The cell should own exactly one mark.");
            Assert.That(marks[0].transform.parent, Is.EqualTo(_sut.transform), "The cell should parent the mark it owns.");
        }

        [Test]
        public void Showing_a_second_mark_keeps_at_most_one_mark_under_the_cell()
        {
            // Arrange
            _sut.ShowMark(Mark.X);

            // Act
            _sut.ShowMark(Mark.O);

            // Assert
            Assert.That(_sut.GetComponentsInChildren<MarkView>(), Has.Length.EqualTo(1), "The cell should own at most one mark.");
        }

        [UnityTest]
        public IEnumerator Clearing_a_marked_cell_destroys_only_its_owned_mark()
        {
            // Arrange
            _sut.ShowMark(Mark.X);

            // Act
            _sut.ClearMark();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _sut.GetComponentInChildren<MarkView>() == null,
                "Clearing a marked cell should destroy its mark.");
            Assert.That(_sut != null, Is.True, "Clearing a mark should preserve the cell itself.");
        }

        [Test]
        public void Clearing_an_unmarked_cell_repeatedly_is_harmless()
        {
            // Act
            TestDelegate clearUnmarkedCell = () =>
            {
                _sut.ClearMark();
                _sut.ClearMark();
            };

            // Assert
            Assert.That(clearUnmarkedCell, Throws.Nothing);
            Assert.That(_sut.GetComponentInChildren<MarkView>(), Is.Null, "Clearing an unmarked cell should leave no mark.");
        }
    }
}
#endif
