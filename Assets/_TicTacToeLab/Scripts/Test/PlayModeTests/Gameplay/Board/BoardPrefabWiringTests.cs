#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardPrefabWiringTests
    {
        private const int BOARD_DIMENSION = 3;
        private const string BOARD_VIEW_PREFAB_PATH = "Assets/_TicTacToeLab/Prefabs/Gameplay/BoardView.prefab";

        private readonly List<GameObject> _instances = new();

        [UnityTearDown]
        public IEnumerator DestroyPrefabInstances()
        {
            foreach (GameObject instance in _instances)
            {
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _instances.All(instance => instance == null),
                "The production prefab instances should be destroyed after the test.");

            _instances.Clear();
        }

        [Test]
        public void The_production_board_prefab_builds_cells_and_marks_from_its_wired_prefab_dependencies()
        {
            // Arrange
            BoardView sut = InstantiateProductionComponent<BoardView>(BOARD_VIEW_PREFAB_PATH);
            BoardLayout boardLayout = new(BOARD_DIMENSION);

            // Act
            sut.Construct(BOARD_DIMENSION, boardLayout.GetCellPlacements());
            sut.ShowMark(new CellCoordinate(0, 1), Mark.X);

            // Assert
            Assert.That(sut.GetComponentsInChildren<CellView>(), Has.Length.EqualTo(9), "The production board prefab should create its cells from its wired cell prefab.");
            Transform addressedCell = sut.transform.Find("Cell (0, 1)");
            Assert.That(addressedCell, Is.Not.Null, "The production board prefab should create the addressed cell.");
            Assert.That(addressedCell.GetComponentInChildren<MarkView>(), Is.Not.Null, "The production cell prefab should create its mark from its wired mark prefab.");
        }

        private T InstantiateProductionComponent<T>(string prefabPath) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, $"The production prefab must exist at {prefabPath}.");

            GameObject instance = Object.Instantiate(prefab);
            _instances.Add(instance);

            T component = instance.GetComponent<T>();
            Assert.That(component, Is.Not.Null, $"The production prefab at {prefabPath} must carry {typeof(T).Name}.");
            return component;
        }
    }
}
#endif
