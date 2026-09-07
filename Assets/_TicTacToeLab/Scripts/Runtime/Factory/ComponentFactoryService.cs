using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class ComponentFactoryService : MonoBehaviour, IComponentFactoryService
    {
        [SerializeField] private GameObject[] _prefabs;

        private readonly Dictionary<Type, Component> _prefabCache = new();

        public T Get<T>() where T : Component
        {
            return Instantiate(ResolveComponent<T>());
        }

        public T Get<T>(Transform parent) where T : Component
        {
            return Instantiate(ResolveComponent<T>(), parent);
        }

        public void Return<T>(T instance) where T : Component
        {
            if (instance == null)
            {
                return;
            }

            Destroy(instance.gameObject);
        }

        private T ResolveComponent<T>() where T : Component
        {
            Type type = typeof(T);
            if (_prefabCache.TryGetValue(type, out Component cachedComponent))
            {
                return (T)cachedComponent;
            }

            if (_prefabs != null)
            {
                foreach (GameObject prefab in _prefabs)
                {
                    if (prefab.TryGetComponent(out T component))
                    {
                        _prefabCache[type] = component;
                        return component;
                    }
                }
            }

            throw new InvalidOperationException($"No prefab is registered for spawnable type '{type.Name}'.");
        }
    }
}
