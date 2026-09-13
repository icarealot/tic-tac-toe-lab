#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// A narrow factory stand-in for the isolated board and cell fixtures: it instantiates real
    /// components for the two roles those fixtures request and parents them as asked. Any other
    /// role is a test error, keeping the stand-in narrower than the production registry. Created
    /// objects are parented under the fixture's own root, so the fixture's teardown owns them.
    /// </summary>
    public sealed class TestComponentFactory : IFactoryService
    {
        public List<Component> Returned { get; } = new();

        public T Get<T>() where T : class
        {
            throw new NotSupportedException("Component fixtures parent every object they create; request the role with a parent.");
        }

        public T Get<T>(Transform parent) where T : class
        {
            Type componentType = typeof(T) == typeof(ICellView) ? typeof(CellView)
                : typeof(T) == typeof(IMarkView) ? typeof(MarkView)
                : throw new NotSupportedException($"Component fixtures only create {nameof(ICellView)} and {nameof(IMarkView)} roles.");

            GameObject host = new(componentType.Name);
            host.transform.SetParent(parent, false);

            return (T)(object)host.AddComponent(componentType);
        }

        /// <summary>
        /// Records the handed-back instance and destroys its GameObject at the end of the current
        /// frame — the Unity destruction boundary the production factory also returns objects on.
        /// </summary>
        public void Return<T>(T instance) where T : class
        {
            Component component = (Component)(object)instance;
            Returned.Add(component);
            UnityEngine.Object.Destroy(component.gameObject);
        }
    }
}
#endif
