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
        public IEnumerator Clearing_a_cell_holding_a_mark_returns_that_mark_view_through_the_component_factory_service()
        {
            yield return IE_LoadScene();

            CellView cellView = GameObject.Find("Cell (0, 0)").GetComponent<CellView>();
            ComponentFactoryService componentFactoryService = Object.FindFirstObjectByType<ComponentFactoryService>();
            RecordingComponentFactoryService recordingFactory = new(componentFactoryService);
            cellView.ShowMark(recordingFactory, Mark.X);
            MarkView markView = cellView.GetComponentInChildren<MarkView>();

            Assert.That(markView, Is.Not.Null);

            cellView.ClearMark(recordingFactory);
            Assert.That(recordingFactory.ReturnedInstance, Is.SameAs(markView));

            yield return null;

            Assert.That(cellView.GetComponentInChildren<MarkView>(), Is.Null);
        }

        private sealed class RecordingComponentFactoryService : IComponentFactoryService
        {
            private readonly IComponentFactoryService _componentFactoryService;

            public RecordingComponentFactoryService(IComponentFactoryService componentFactoryService)
            {
                _componentFactoryService = componentFactoryService;
            }

            public Component ReturnedInstance { get; private set; }

            public T Get<T>() where T : Component
            {
                return _componentFactoryService.Get<T>();
            }

            public T Get<T>(Transform parent) where T : Component
            {
                return _componentFactoryService.Get<T>(parent);
            }

            public Component Get(System.Type componentType, Transform parent)
            {
                return _componentFactoryService.Get(componentType, parent);
            }

            public void Return<T>(T instance) where T : Component
            {
                ReturnedInstance = instance;
                _componentFactoryService.Return(instance);
            }
        }
    }
}
#endif
