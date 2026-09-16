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
            // Arrange
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return IE_StartGameThroughEventSystem(mouse);

            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            Transform emptyCell = Cell(0, 0);
            Assert.That(MarkAt(emptyCell), Is.Null);

            // Act
            // X owns the first turn; this journey checks that the routed press materializes it.
            yield return IE_PressCell(mouse, emptyCell);

            // Assert
            MarkView shownX = MarkAt(emptyCell);
            Assert.That(shownX, Is.Not.Null);
            Assert.That(shownX.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_real_win_shows_its_outcome_popup_and_continue_leads_to_a_menu_that_resets_on_start()
        {
            // Arrange
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            yield return IE_StartGameThroughButtonEvent();

            BoardView boardBeforeReset = Object.FindFirstObjectByType<BoardView>();
            Transform firstCellBeforeReset = Cell(0, 0);

            // Act
            yield return IE_CompleteXWin(mouse);
            Sprite xSprite = MarkAt(firstCellBeforeReset).GetComponent<SpriteRenderer>().sprite;
            yield return IE_WaitUntil(
                () => CurrentOutcomePopup(uiRoot) != null,
                "The outcome popup should appear after the completed-game pause.");

            // Assert
            OutcomePopup outcomePopup = CurrentOutcomePopup(uiRoot);
            UnityEngine.UI.Button continueButton = outcomePopup.ContinueButton;
            Assert.That(continueButton, Is.Not.Null, "The outcome popup should provide a button wired to Continue.");
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "The popup should appear over the completed board, not a reset one.");

            // Act
            continueButton.onClick.Invoke();
            yield return IE_WaitUntil(
                () => CurrentOutcomePopup(uiRoot) == null && Object.FindFirstObjectByType<MainMenuPanel>() != null,
                "Continue should close the outcome popup and show the main menu.");

            // Assert
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "Returning to the menu should preserve the completed board.");

            // Act
            yield return IE_StartGameThroughButtonEvent();

            // Assert
            // Start resets the same board session in place before gameplay begins.
            Assert.That(Object.FindFirstObjectByType<BoardView>(), Is.SameAs(boardBeforeReset));
            Assert.That(Cell(0, 0), Is.SameAs(firstCellBeforeReset));
            Assert.That(VisibleMarks(), Is.Empty);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            // Act
            yield return IE_PressCell(mouse, firstCellBeforeReset);

            // Assert
            MarkView freshX = MarkAt(firstCellBeforeReset);
            Assert.That(freshX, Is.Not.Null);
            Assert.That(freshX.GetComponent<SpriteRenderer>().sprite, Is.SameAs(xSprite));
        }

        [UnityTest]
        public IEnumerator Back_opens_quit_confirmation_and_no_resumes_the_game()
        {
            // Arrange
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Transform firstCell = Cell(0, 0);
            Transform secondCell = Cell(0, 1);
            yield return IE_StartGameThroughButtonEvent();
            yield return IE_PressCell(mouse, firstCell);

            // Act
            yield return IE_PressBack(keyboard);
            yield return IE_WaitUntil(
                () => CurrentConfirmation(uiRoot) != null,
                "Back should open quit confirmation during gameplay.");

            // Assert
            ConfirmQuitPopup confirmation = CurrentConfirmation(uiRoot);
            Assert.That(MarkAt(secondCell), Is.Null);

            // Act
            confirmation.NoButton.onClick.Invoke();
            yield return IE_WaitUntil(
                () => CurrentConfirmation(uiRoot) == null,
                "Answering No should close quit confirmation.");
            yield return IE_PressCell(mouse, secondCell);

            // Assert
            Assert.That(MarkAt(secondCell), Is.Not.Null, "Answering No should restore board input.");
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);
        }

        private static Transform Cell(int row, int column)
        {
            return GameObject.Find($"Cell ({row}, {column})").transform;
        }

        private IEnumerator IE_CompleteXWin(Mouse mouse)
        {
            // X wins row 0; O uses row 1 between X's turns.
            yield return IE_PressCell(mouse, Cell(0, 0));
            yield return IE_PressCell(mouse, Cell(1, 0));
            yield return IE_PressCell(mouse, Cell(0, 1));
            yield return IE_PressCell(mouse, Cell(1, 1));
            yield return IE_PressCell(mouse, Cell(0, 2));
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

        private static OutcomePopup CurrentOutcomePopup(UIRoot uiRoot)
        {
            return uiRoot.PopupLayer.GetComponentInChildren<OutcomePopup>();
        }

    }
}
#endif
