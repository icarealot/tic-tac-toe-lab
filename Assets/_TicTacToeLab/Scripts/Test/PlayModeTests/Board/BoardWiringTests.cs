#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardWiringTests : InputTestFixture
    {
        private const string SCENE_PATH = "Assets/_TicTacToeLab/Scenes/Main.unity";

        [UnityTest]
        public IEnumerator A_press_over_a_cell_in_the_real_scene_spawns_a_mark_view_under_that_cells_view()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Mouse mouse = InputSystem.AddDevice<Mouse>();

            Transform firstCellTransform = GameObject.Find("Cell (0, 0)").transform;
            Transform secondCellTransform = GameObject.Find("Cell (0, 1)").transform;

            yield return IE_PressCell(mouse, firstCellTransform);
            yield return IE_PressCell(mouse, secondCellTransform);

            SpriteRenderer firstSpriteRenderer = firstCellTransform.GetComponentInChildren<MarkView>().GetComponent<SpriteRenderer>();
            SpriteRenderer secondSpriteRenderer = secondCellTransform.GetComponentInChildren<MarkView>().GetComponent<SpriteRenderer>();

            Assert.That(firstSpriteRenderer.sprite, Is.Not.Null);
            Assert.That(secondSpriteRenderer.sprite, Is.Not.Null);
            Assert.That(firstSpriteRenderer.sprite, Is.Not.EqualTo(secondSpriteRenderer.sprite));
        }

        [UnityTest]
        public IEnumerator Winning_a_game_holds_the_board_for_the_pause_then_clears_it_for_a_fresh_press()
        {
            yield return EditorSceneManager.LoadSceneInPlayMode(SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            Mouse mouse = InputSystem.AddDevice<Mouse>();

            // X: (0,0) (0,1) (0,2) win row 0; O: (1,0) (1,1) in between.
            yield return IE_PressCell(mouse, GameObject.Find("Cell (0, 0)").transform);
            yield return IE_PressCell(mouse, GameObject.Find("Cell (1, 0)").transform);
            yield return IE_PressCell(mouse, GameObject.Find("Cell (0, 1)").transform);
            yield return IE_PressCell(mouse, GameObject.Find("Cell (1, 1)").transform);
            yield return IE_PressCell(mouse, GameObject.Find("Cell (0, 2)").transform);

            yield return new WaitForSeconds(GameCompleteState.RESET_PAUSE_SECONDS + 0.25f);

            MarkView[] remainingMarks = Object.FindObjectsByType<MarkView>(FindObjectsSortMode.None);
            Assert.That(remainingMarks, Is.Empty);

            Transform freshCellTransform = GameObject.Find("Cell (0, 0)").transform;
            yield return IE_PressCell(mouse, freshCellTransform);

            SpriteRenderer spriteRenderer = freshCellTransform.GetComponentInChildren<MarkView>().GetComponent<SpriteRenderer>();
            Assert.That(spriteRenderer.sprite, Is.Not.Null);
        }

        private IEnumerator IE_PressCell(Mouse mouse, Transform cellTransform)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(cellTransform.position);

            Set(mouse.position, new Vector2(screenPoint.x, screenPoint.y));
            Press(mouse.leftButton);
            Release(mouse.leftButton);

            yield return null;
        }
    }
}
#endif
