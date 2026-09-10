#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CellViewTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator A_cell_obtained_through_its_role_applies_its_assigned_local_point_and_diagnostic_name()
        {
            yield return IE_LoadScene();

            CellPlacement placement = new(1, 2, new Vector3(0.3f, -0.4f, 0f));
            GameObject host = new("CellHost");
            RecordingFactoryService factory = new(Object.FindFirstObjectByType<FactoryService>());
            ICellView cellView = factory.Get<ICellView>(host.transform);
            cellView.Construct(factory, placement);
            CellView cell = (CellView)cellView;

            Assert.That(cell.name, Is.EqualTo("Cell (1, 2)"));
            Assert.That(cell.transform.localPosition, Is.EqualTo(placement.LocalPoint));

            factory.Return(cellView);
            Object.Destroy(host);
        }

        [UnityTest]
        public IEnumerator Showing_a_mark_requests_an_interface_based_mark_beneath_the_cell_and_displays_the_requested_mark()
        {
            yield return IE_LoadScene();

            GameObject host = new("CellHost");
            RecordingFactoryService factory = new(Object.FindFirstObjectByType<FactoryService>());
            ICellView cellView = factory.Get<ICellView>(host.transform);
            cellView.Construct(factory, new CellPlacement(0, 0, Vector3.zero));
            CellView cell = (CellView)cellView;

            cellView.ShowMark(Mark.X);

            MarkView markView = cell.GetComponentInChildren<MarkView>();
            Assert.That(markView, Is.Not.Null);
            Assert.That(markView.transform.parent, Is.EqualTo(cell.transform));
            Assert.That(markView.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(factory.GetRequests, Does.Contain((typeof(IMarkView), cell.transform)));

            factory.Return(cellView);
            Object.Destroy(host);
        }

        [UnityTest]
        public IEnumerator Clearing_a_marked_cell_returns_that_same_mark_through_the_factory_service()
        {
            yield return IE_LoadScene();

            GameObject host = new("CellHost");
            RecordingFactoryService factory = new(Object.FindFirstObjectByType<FactoryService>());
            ICellView cellView = factory.Get<ICellView>(host.transform);
            cellView.Construct(factory, new CellPlacement(0, 0, Vector3.zero));
            CellView cell = (CellView)cellView;

            cellView.ShowMark(Mark.O);
            MarkView markView = cell.GetComponentInChildren<MarkView>();
            Assert.That(markView, Is.Not.Null);

            cellView.ClearMark();
            Assert.That(factory.ReturnedInstance, Is.SameAs(markView));

            yield return null;

            Assert.That(cell.GetComponentInChildren<MarkView>(), Is.Null);

            factory.Return(cellView);
            Object.Destroy(host);
        }
    }
}
#endif
