using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class FactoryService : MonoBehaviour, IFactoryService
    {
        [SerializeField] private GameObject[] _prefabs;

        public T Get<T>() where T : class
        {
            return (T)GetComponentInstance(typeof(T), null);
        }

        public T Get<T>(Transform parent) where T : class
        {
            return (T)GetComponentInstance(typeof(T), parent);
        }

        public void Return<T>(T instance) where T : class
        {
            if (instance == null)
            {
                return;
            }

            if (instance is not Component component)
            {
                throw new InvalidOperationException(
                    $"Instance of type '{instance.GetType().Name}' is not a Unity component and cannot be returned to the factory.");
            }

            Destroy(component.gameObject);
        }

        private object GetComponentInstance(Type requestedType, Transform parent)
        {
            if (!requestedType.IsInterface)
            {
                throw new InvalidOperationException(
                    $"The requested type '{requestedType.Name}' is not an interface; factory requests must name an interface role.");
            }

            Component implementation = ResolveComponent(requestedType);

            return parent == null
                ? Instantiate(implementation)
                : Instantiate(implementation, parent);
        }

        private Component ResolveComponent(Type requestedType)
        {
            Component match = null;

            if (_prefabs != null)
            {
                foreach (GameObject prefab in _prefabs)
                {
                    if (prefab == null)
                    {
                        continue;
                    }

                    if (prefab.TryGetComponent(requestedType, out Component component))
                    {
                        if (match != null)
                        {
                            throw new InvalidOperationException(
                                $"Multiple registered prefabs implement interface '{requestedType.Name}': '{match.name}' and '{component.name}'.");
                        }

                        match = component;
                    }
                }
            }

            if (match == null)
            {
                throw new InvalidOperationException($"No prefab is registered for interface '{requestedType.Name}'.");
            }

            return match;
        }
    }
}
