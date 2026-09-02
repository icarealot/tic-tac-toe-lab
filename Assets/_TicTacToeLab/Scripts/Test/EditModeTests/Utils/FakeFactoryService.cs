using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeFactoryService : IFactoryService
    {
        public List<Component> ReturnedInstances { get; } = new();

        public T Get<T>() where T : Component
        {
            GameObject gameObject = new(typeof(T).Name);
            return gameObject.AddComponent<T>();
        }

        public T Get<T>(Transform parent) where T : Component
        {
            T instance = Get<T>();
            instance.transform.SetParent(parent);
            return instance;
        }

        public void Return<T>(T instance) where T : Component
        {
            ReturnedInstances.Add(instance);
            Object.DestroyImmediate(instance.gameObject);
        }
    }
}
