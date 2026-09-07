using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeComponentFactoryService : IComponentFactoryService
    {
        private const BindingFlags DECLARED_FIELDS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public List<Component> ReturnedInstances { get; } = new();

        public T Get<T>() where T : Component
        {
            GameObject gameObject = new(typeof(T).Name);
            T instance = gameObject.AddComponent<T>();
            SupplyTextChildren(instance);
            return instance;
        }

        public T Get<T>(Transform parent) where T : Component
        {
            T instance = Get<T>();
            instance.transform.SetParent(parent);
            return instance;
        }

        public Component Get(Type componentType, Transform parent)
        {
            if (componentType == typeof(IGameplayPanel))
            {
                return Get<GameplayPanel>(parent);
            }

            if (componentType == typeof(IConfirmQuitPopup))
            {
                return Get<ConfirmQuitPopup>(parent);
            }

            throw new InvalidOperationException($"No fake component is registered for type '{componentType.Name}'.");
        }

        public void Return<T>(T instance) where T : Component
        {
            ReturnedInstances.Add(instance);
            UnityEngine.Object.DestroyImmediate(instance.gameObject);
        }

        // The real component factory hands back a prefab instance whose children are already wired to its
        // serialized fields. A bare component has none, so supply the text a window writes to.
        private static void SupplyTextChildren(Component instance)
        {
            for (Type type = instance.GetType(); type != null && type != typeof(Component); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(DECLARED_FIELDS))
                {
                    if (!typeof(TMP_Text).IsAssignableFrom(field.FieldType))
                    {
                        continue;
                    }

                    GameObject child = new(field.Name);
                    child.transform.SetParent(instance.transform);
                    field.SetValue(instance, child.AddComponent<TextMeshProUGUI>());
                }
            }
        }
    }
}
