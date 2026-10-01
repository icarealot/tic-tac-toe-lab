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

        private readonly List<GameObject> _productionInstances = new();

        [UnityTearDown]
        public IEnumerator DestroyProductionInstances()
        {
            GameObject[] instances = _productionInstances.ToArray();
            _productionInstances.Clear();

            foreach (GameObject instance in instances)
            {
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => instances.All(instance => instance == null),
                "Every instantiated production object should be destroyed after the test.");
        }

        [Test]
        public void Showing_X_and_O_through_the_production_board_chain_creates_owned_marks_with_usable_distinct_sprites()
        {
            // Arrange
            BoardView sut = InstantiateProductionComponent<BoardView>(BOARD_VIEW_PREFAB_PATH);
            BoardLayout layout = new(BOARD_DIMENSION);
            sut.Construct(BOARD_DIMENSION, layout.GetCellPlacements());
            CellCoordinate xCoordinate = new(0, 2);
            CellCoordinate oCoordinate = new(2, 0);

            // Act
            sut.ShowMark(xCoordinate, Mark.X);
            sut.ShowMark(oCoordinate, Mark.O);

            // Assert
            MarkView xMark = FindOwnedMark(sut, layout, xCoordinate);
            MarkView oMark = FindOwnedMark(sut, layout, oCoordinate);
            Assert.That(
                sut.GetComponentsInChildren<MarkView>(),
                Has.Length.EqualTo(2),
                "Showing marks should create marks only in the addressed cells.");

            SpriteRenderer xRenderer = xMark.GetComponent<SpriteRenderer>();
            SpriteRenderer oRenderer = oMark.GetComponent<SpriteRenderer>();
            Assert.That(xRenderer, Is.Not.Null, "The mark shown for X should carry the renderer that shows it.");
            Assert.That(oRenderer, Is.Not.Null, "The mark shown for O should carry the renderer that shows it.");
            Assert.That(xRenderer.sprite, Is.Not.Null, "Showing X through the production board should reach a mark that renders a sprite.");
            Assert.That(oRenderer.sprite, Is.Not.Null, "Showing O through the production board should reach a mark that renders a sprite.");
            Assert.That(xRenderer.sprite, Is.Not.SameAs(oRenderer.sprite), "X and O should render distinct sprites through the production board chain.");
        }

        private static MarkView FindOwnedMark(BoardView board, BoardLayout layout, CellCoordinate coordinate)
        {
            CellView addressedCell = FindAddressedCell(board, layout, coordinate);
            MarkView[] ownedMarks = addressedCell.GetComponentsInChildren<MarkView>();
            Assert.That(
                ownedMarks,
                Has.Length.EqualTo(1),
                $"The addressed cell at ({coordinate.Row}, {coordinate.Column}) should own exactly one mark.");

            return ownedMarks[0];
        }

        private static CellView FindAddressedCell(BoardView board, BoardLayout layout, CellCoordinate coordinate)
        {
            CellView[] matchingCells = board.GetComponentsInChildren<CellView>()
                .Where(cell => TryResolveAddress(board, layout, cell, out CellCoordinate resolvedCoordinate)
                    && resolvedCoordinate == coordinate)
                .ToArray();

            Assert.That(
                matchingCells,
                Has.Length.EqualTo(1),
                $"The production board should address exactly one cell for ({coordinate.Row}, {coordinate.Column}).");

            return matchingCells[0];
        }

        private static bool TryResolveAddress(BoardView board, BoardLayout layout, CellView cell, out CellCoordinate coordinate)
        {
            Vector3 boardLocalPoint = board.ToLocalPoint(cell.transform.position);
            return layout.TryResolvePoint(boardLocalPoint, out coordinate);
        }

        private T InstantiateProductionComponent<T>(string prefabPath) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null, $"The production prefab must exist at {prefabPath}.");

            GameObject instance = Object.Instantiate(prefab);
            _productionInstances.Add(instance);

            T component = instance.GetComponent<T>();
            Assert.That(component, Is.Not.Null, $"The production prefab at {prefabPath} must carry {typeof(T).Name}.");
            return component;
        }
    }
}
#endif
