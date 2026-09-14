#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class CriticalJourneyTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator Starting_through_the_real_menu_and_pressing_an_empty_cell_shows_X()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return IE_StartGame(mouse);

            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            Transform emptyCell = Cell(0, 0);
            Assert.That(MarkAt(emptyCell), Is.Null);

            // X owns the first turn; this journey checks that the routed press materializes it.
            yield return IE_PressCell(mouse, emptyCell);

            MarkView shownX = MarkAt(emptyCell);
            Assert.That(shownX, Is.Not.Null);
            Assert.That(shownX.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_real_win_stays_visible_for_the_pause_resets_in_place_and_accepts_a_fresh_X()
        {
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return IE_StartGame(mouse);

            BoardView boardBeforeReset = Object.FindFirstObjectByType<BoardView>();
            Transform firstCellBeforeReset = Cell(0, 0);

            // X wins row 0; O uses row 1 between X's turns.
            yield return IE_PressCell(mouse, Cell(0, 0));
            Sprite xSprite = MarkAt(firstCellBeforeReset).GetComponent<SpriteRenderer>().sprite;
            yield return IE_PressCell(mouse, Cell(1, 0));
            yield return IE_PressCell(mouse, Cell(0, 1));
            yield return IE_PressCell(mouse, Cell(1, 1));
            yield return IE_PressCell(mouse, Cell(0, 2));

            Assert.That(VisibleMarks(), Has.Length.EqualTo(5));

            yield return new WaitForSeconds(GameCompleteState.RESET_PAUSE_SECONDS * 0.5f);

            Assert.That(VisibleMarks(), Has.Length.EqualTo(5),
                "The completed game should remain visible during the reset pause.");

            yield return new WaitForSeconds(GameCompleteState.RESET_PAUSE_SECONDS * 0.5f + 0.25f);

            Assert.That(VisibleMarks(), Is.Empty);
            Assert.That(Object.FindFirstObjectByType<BoardView>(), Is.SameAs(boardBeforeReset));
            Assert.That(Cell(0, 0), Is.SameAs(firstCellBeforeReset));
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            yield return IE_PressCell(mouse, firstCellBeforeReset);

            MarkView freshX = MarkAt(firstCellBeforeReset);
            Assert.That(freshX, Is.Not.Null);
            Assert.That(freshX.GetComponent<SpriteRenderer>().sprite, Is.SameAs(xSprite));
        }

        [UnityTest]
        public IEnumerator Quit_confirmation_no_resumes_yes_returns_to_menu_and_the_next_start_is_fresh()
        {
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Transform firstCell = Cell(0, 0);
            Transform secondCell = Cell(0, 1);

            yield return IE_StartGame(mouse);
            yield return IE_PressCell(mouse, firstCell);
            Sprite xSprite = MarkAt(firstCell).GetComponent<SpriteRenderer>().sprite;

            yield return IE_PressBack(keyboard);
            ConfirmQuitPopup firstConfirmation = CurrentConfirmation(uiRoot);
            Assert.That(firstConfirmation, Is.Not.Null);

            yield return IE_PressCell(mouse, secondCell);
            Assert.That(MarkAt(secondCell), Is.Null,
                "Board input should remain blocked while quit confirmation is present.");

            yield return IE_ClickButton(mouse, NoButton(firstConfirmation));
            Assert.That(CurrentConfirmation(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            yield return IE_PressCell(mouse, secondCell);
            Assert.That(MarkAt(secondCell), Is.Not.Null,
                "Answering No should restore board input.");

            yield return IE_PressBack(keyboard);
            ConfirmQuitPopup secondConfirmation = CurrentConfirmation(uiRoot);
            Assert.That(secondConfirmation, Is.Not.Null);

            yield return IE_ClickButton(mouse, YesButton(secondConfirmation));

            Assert.That(CurrentConfirmation(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Null);
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Not.Null);
            Assert.That(VisibleMarks(), Has.Length.EqualTo(2),
                "Returning to the menu should preserve the abandoned game until the next start.");

            yield return IE_StartGame(mouse);

            Assert.That(VisibleMarks(), Is.Empty);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            yield return IE_PressCell(mouse, firstCell);

            MarkView freshX = MarkAt(firstCell);
            Assert.That(freshX, Is.Not.Null);
            Assert.That(freshX.GetComponent<SpriteRenderer>().sprite, Is.SameAs(xSprite));
        }

        private static Transform Cell(int row, int column)
        {
            return GameObject.Find($"Cell ({row}, {column})").transform;
        }

        private static MarkView MarkAt(Transform cell)
        {
            return cell.GetComponentInChildren<MarkView>();
        }

        private static MarkView[] VisibleMarks()
        {
            return Object.FindObjectsByType<MarkView>(FindObjectsSortMode.None);
        }

        private static ConfirmQuitPopup CurrentConfirmation(UIRoot uiRoot)
        {
            return uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();
        }

        private static UnityEngine.UI.Button YesButton(ConfirmQuitPopup popup)
        {
            return popup.transform.Find("SafeArea/Question/YesButton").GetComponent<UnityEngine.UI.Button>();
        }

        private static UnityEngine.UI.Button NoButton(ConfirmQuitPopup popup)
        {
            return popup.transform.Find("SafeArea/Question/NoButton").GetComponent<UnityEngine.UI.Button>();
        }
    }
}
#endif
