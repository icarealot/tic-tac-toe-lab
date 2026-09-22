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
        public IEnumerator A_real_win_shows_its_outcome_popup_and_continue_leads_to_a_menu_that_resets_on_start()
        {
            // Arrange
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            ApplicationUI applicationUI = Object.FindFirstObjectByType<ApplicationUI>();
            yield return IE_StartGameThroughEventSystem(mouse);

            BoardView boardBeforeReset = Object.FindFirstObjectByType<BoardView>();
            Transform firstCellBeforeReset = Cell(new CellCoordinate(0, 0));
            Assert.That(MarkAt(firstCellBeforeReset), Is.Null, "A newly started game should show an empty board.");

            // Act
            yield return IE_CompleteXWin(mouse);
            Sprite xSprite = MarkAt(firstCellBeforeReset).GetComponent<SpriteRenderer>().sprite;
            Assert.That(xSprite, Is.Not.Null, "A placed X should show its wired sprite.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => CurrentOutcomePopup(applicationUI) != null,
                "The outcome popup should appear after the completed-game pause.");

            // Assert
            OutcomePopup outcomePopup = CurrentOutcomePopup(applicationUI);
            UnityEngine.UI.Button continueButton = TestSerializedReference.ReadButton(outcomePopup, "_continueButton");
            Assert.That(continueButton, Is.Not.Null, "The outcome popup should provide a button wired to Continue.");
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "The popup should appear over the completed board, not a reset one.");

            // Act
            continueButton.onClick.Invoke();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => CurrentOutcomePopup(applicationUI) == null && Object.FindFirstObjectByType<MainMenuPanel>() != null,
                "Continue should close the outcome popup and show the main menu.");

            // Assert
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "Returning to the menu should preserve the completed board.");

            // Act
            yield return IE_StartGameThroughButtonEvent();

            // Assert
            // Start resets the same board session in place before gameplay begins.
            Assert.That(Object.FindFirstObjectByType<BoardView>(), Is.SameAs(boardBeforeReset));
            Assert.That(Cell(new CellCoordinate(0, 0)), Is.SameAs(firstCellBeforeReset));
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
        public IEnumerator Back_reaches_quit_confirmation_through_the_production_input_route()
        {
            // Arrange
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            ApplicationUI applicationUI = Object.FindFirstObjectByType<ApplicationUI>();
            yield return IE_StartGameThroughButtonEvent();

            // Act
            yield return IE_PressBack(keyboard);

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => CurrentConfirmation(applicationUI) != null,
                "Back should open quit confirmation during gameplay.");
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null, "Gameplay should remain present beneath the confirmation.");
        }

        private static Transform Cell(CellCoordinate coordinate)
        {
            return GameObject.Find($"Cell ({coordinate.Row}, {coordinate.Column})").transform;
        }

        private IEnumerator IE_CompleteXWin(Mouse mouse)
        {
            // X wins row 0; O uses row 1 between X's turns.
            yield return IE_PressCell(mouse, Cell(new CellCoordinate(0, 0)));
            yield return IE_PressCell(mouse, Cell(new CellCoordinate(1, 0)));
            yield return IE_PressCell(mouse, Cell(new CellCoordinate(0, 1)));
            yield return IE_PressCell(mouse, Cell(new CellCoordinate(1, 1)));
            yield return IE_PressCell(mouse, Cell(new CellCoordinate(0, 2)));
        }

        private static MarkView MarkAt(Transform cell)
        {
            return cell.GetComponentInChildren<MarkView>();
        }

        private static MarkView[] VisibleMarks()
        {
            return Object.FindObjectsByType<MarkView>(FindObjectsSortMode.None);
        }

        private static ConfirmQuitPopup CurrentConfirmation(ApplicationUI applicationUI)
        {
            return applicationUI.GetComponentInChildren<ConfirmQuitPopup>(includeInactive: true);
        }

        private static OutcomePopup CurrentOutcomePopup(ApplicationUI applicationUI)
        {
            return applicationUI.GetComponentInChildren<OutcomePopup>(includeInactive: true);
        }

    }
}
#endif
