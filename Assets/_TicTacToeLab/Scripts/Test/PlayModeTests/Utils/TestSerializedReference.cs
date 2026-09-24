#if UNITY_EDITOR
using NUnit.Framework;
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

        public static void ClearReference(Component owner, string fieldName)
        {
            AssignPrefab(owner, fieldName, null);
        }
    }
}
#endif
