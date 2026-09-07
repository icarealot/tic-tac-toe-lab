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

        public Component Get(Type componentType, Transform parent)
        {
            return Instantiate(ResolveComponent(componentType), parent);
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
            return (T)ResolveComponent(typeof(T));
        }

        private Component ResolveComponent(Type type)
        {
            if (_prefabCache.TryGetValue(type, out Component cachedComponent))
            {
                return cachedComponent;
            }

            if (_prefabs != null)
            {
                foreach (GameObject prefab in _prefabs)
                {
                    if (prefab.TryGetComponent(type, out Component component))
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
