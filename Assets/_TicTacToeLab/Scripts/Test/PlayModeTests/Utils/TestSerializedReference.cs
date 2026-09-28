#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public static class TestSerializedReference
    {
        public static void AssignPrefab(Component owner, string fieldName, Object prefab)
        {
            SerializedObject serializedOwner = new(owner);
            SerializedProperty field = serializedOwner.FindProperty(fieldName);
            Assert.That(field, Is.Not.Null, $"The serialized field '{fieldName}' should exist on {owner.GetType().Name}.");

            field.objectReferenceValue = prefab;
            _ = serializedOwner.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AssignScreenRegistrations(AppUI owner, IReadOnlyList<ScreenRegistration> registrations)
        {
            SerializedObject serializedOwner = new(owner);
            SerializedProperty list = serializedOwner.FindProperty("_screenRegistrations");
            Assert.That(list, Is.Not.Null, "AppUI should serialize its screen registrations.");

            list.arraySize = registrations.Count;
            for (int index = 0; index < registrations.Count; index++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("_prefab").objectReferenceValue = registrations[index].Prefab;
                element.FindPropertyRelative("_layer").enumValueIndex = (int)registrations[index].Layer;
            }

            _ = serializedOwner.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void ClearReference(Component owner, string fieldName)
        {
            AssignPrefab(owner, fieldName, null);
        }
    }
}
#endif
