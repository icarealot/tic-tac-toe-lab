#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CellViewTests
    {
        private GeneratedBoardFixture _fixture;
        private CellView _sut;

        [SetUp]
        public void CreateGeneratedCell()
        {
            _fixture = new GeneratedBoardFixture();
            _sut = _fixture.CreateCell(new CellCoordinate(0, 0));
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedCell()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Showing_a_mark_creates_exactly_one_live_mark_owned_by_the_cell()
        {
            // Act
            _sut.ShowMark(Mark.X);

            // Assert
            Assert.That(
                _sut.GetComponentsInChildren<MarkView>(),
                Has.Length.EqualTo(1),
                "The cell should own exactly one live mark after showing a mark.");
        }

        [Test]
        public void Showing_a_second_mark_keeps_at_most_one_live_mark_owned_by_the_cell()
        {
            // Arrange
            _sut.ShowMark(Mark.X);

            // Act
            _sut.ShowMark(Mark.O);

            // Assert
            Assert.That(
                _sut.GetComponentsInChildren<MarkView>(),
                Has.Length.EqualTo(1),
                "The cell should own at most one live mark.");
        }

        [UnityTest]
        public IEnumerator Clearing_a_marked_cell_destroys_its_owned_mark_and_preserves_the_cell()
        {
            // Arrange
            _sut.ShowMark(Mark.X);
            MarkView ownedMark = _sut.GetComponentInChildren<MarkView>();
            Assert.That(ownedMark, Is.Not.Null, "The cell should own a mark before clearing it.");

            // Act
            _sut.ClearMark();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => ownedMark == null,
                "Clearing a marked cell should destroy the mark it owns.");
            Assert.That(_sut.GetComponentsInChildren<MarkView>(), Is.Empty, "Clearing the cell should leave no live mark on it.");
            Assert.That(_sut != null, Is.True, "Clearing a mark should preserve the cell.");
        }

        [Test]
        public void Clearing_a_cell_that_owns_no_mark_is_harmless()
        {
            // Act
            TestDelegate clearUnmarkedCell = () => _sut.ClearMark();

            // Assert
            Assert.That(clearUnmarkedCell, Throws.Nothing, "Clearing a cell that owns no mark should be harmless.");
            Assert.That(_sut.GetComponentsInChildren<MarkView>(), Is.Empty, "Clearing an unmarked cell should leave it without a mark.");
        }
    }
}
#endif
