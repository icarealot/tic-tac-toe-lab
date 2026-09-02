using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class CellViewTests
    {
        private CellView CreateCellView()
        {
            GameObject gameObject = new("Cell", typeof(SpriteRenderer));
            return gameObject.AddComponent<CellView>();
        }

        [Test]
        public void Clearing_a_cell_holding_a_mark_returns_that_mark_view_through_the_factory_service()
        {
            CellView cellView = CreateCellView();
            FakeFactoryService fakeFactoryService = new();
            cellView.ShowMark(fakeFactoryService, Mark.X);
            MarkView markView = cellView.GetComponentInChildren<MarkView>();

            cellView.ClearMark(fakeFactoryService);

            Assert.That(fakeFactoryService.ReturnedInstances, Is.EqualTo(new[] { markView }));
        }
    }
}
