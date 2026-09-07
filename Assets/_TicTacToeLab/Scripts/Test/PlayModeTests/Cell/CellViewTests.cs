#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CellViewTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator Clearing_a_cell_holding_a_mark_returns_that_mark_view_through_the_factory_service()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            CellView cellView = GameObject.Find("Cell (0, 0)").GetComponent<CellView>();
            FactoryService factoryService = Object.FindFirstObjectByType<FactoryService>();
            cellView.ShowMark(factoryService, Mark.X);
            MarkView markView = cellView.GetComponentInChildren<MarkView>();

            Assert.That(markView, Is.Not.Null);

            cellView.ClearMark(factoryService);
            yield return null;

            Assert.That(cellView.GetComponentInChildren<MarkView>(), Is.Null);
        }
    }
}
#endif
