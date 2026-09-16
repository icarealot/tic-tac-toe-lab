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
            yield return IE_StartGame(mouse);

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
        public IEnumerator A_real_win_stays_unobstructed_then_shows_its_outcome_popup_and_continue_leads_to_a_menu_that_resets_on_start()
        {
            // Arrange
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            yield return IE_StartGame(mouse);

            BoardView boardBeforeReset = Object.FindFirstObjectByType<BoardView>();
            Transform firstCellBeforeReset = Cell(0, 0);

            // Act
            yield return IE_CompleteXWin(mouse);
            Sprite xSprite = MarkAt(firstCellBeforeReset).GetComponent<SpriteRenderer>().sprite;

            yield return new WaitForSeconds(GameCompleteState.OUTCOME_PRESENTATION_DELAY_SECONDS * 0.5f);

            // Assert
            // The final position stays unobstructed while the outcome pause runs.
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "The completed board should remain visible during the outcome pause.");
            Assert.That(CurrentOutcomePopup(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Null);

            // Act
            yield return new WaitForSeconds(GameCompleteState.OUTCOME_PRESENTATION_DELAY_SECONDS * 0.5f + 0.25f);

            // Assert
            OutcomePopup outcomePopup = CurrentOutcomePopup(uiRoot);
            Assert.That(outcomePopup, Is.Not.Null);

            // The popup presents one interactive button by contract; asserting uniqueness also makes the role-based lookup safe.
            UnityEngine.UI.Button[] popupButtons = outcomePopup.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.That(popupButtons, Has.Length.EqualTo(1), "The outcome popup should present exactly one interactive button.");
            TMPro.TMP_Text[] popupTexts = outcomePopup.GetComponentsInChildren<TMPro.TMP_Text>();
            Assert.That(popupTexts, Has.Exactly(1).Matches<TMPro.TMP_Text>(text => text.text == "X Wins!"), "The outcome popup should announce the winning mark.");
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "The popup should appear over the completed board, not a reset one.");

            // Act
            yield return IE_ClickButton(mouse, popupButtons[0]);

            // Assert
            Assert.That(CurrentOutcomePopup(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Not.Null);
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "Returning to the menu should preserve the completed board.");

            // Act
            yield return IE_StartGame(mouse);

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
        public IEnumerator Routed_back_and_board_presses_do_nothing_during_the_outcome_pause_then_back_returns_to_the_menu_once_the_popup_is_visible()
        {
            // Arrange
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            yield return IE_StartGame(mouse);
            yield return IE_CompleteXWin(mouse);

            yield return new WaitForSeconds(GameCompleteState.OUTCOME_PRESENTATION_DELAY_SECONDS * 0.5f);

            // Act
            yield return IE_PressCell(mouse, Cell(2, 0));
            yield return IE_PressBack(keyboard);

            // Assert
            // Board presses and Back are ignored while the completed board receives its viewing pause.
            Assert.That(MarkAt(Cell(2, 0)), Is.Null, "Board input should stay blocked during the outcome pause.");
            Assert.That(CurrentOutcomePopup(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Null);
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "Back during the pause must not leave the completed game.");

            // Act
            yield return new WaitForSeconds(GameCompleteState.OUTCOME_PRESENTATION_DELAY_SECONDS * 0.5f + 0.25f);
            OutcomePopup outcomePopup = CurrentOutcomePopup(uiRoot);
            Assert.That(outcomePopup, Is.Not.Null, "The outcome popup should be visible once the pause ends.");
            yield return IE_PressCell(mouse, Cell(2, 1));

            // Assert
            Assert.That(MarkAt(Cell(2, 1)), Is.Null, "Board input should stay blocked while the outcome popup is presented.");

            // Act
            yield return IE_PressBack(keyboard);

            // Assert
            Assert.That(CurrentOutcomePopup(uiRoot), Is.Null, "Back acknowledgement should close the popup.");
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Not.Null);
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "Back should preserve the completed board exactly like Continue.");
        }

        [UnityTest]
        public IEnumerator Clicking_the_scrim_outside_the_card_leaves_the_outcome_popup_and_the_completed_board_unchanged()
        {
            // Arrange
            yield return IE_LoadScene();

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            yield return IE_StartGame(mouse);
            yield return IE_CompleteXWin(mouse);

            yield return new WaitForSeconds(GameCompleteState.OUTCOME_PRESENTATION_DELAY_SECONDS + 0.25f);

            OutcomePopup outcomePopup = CurrentOutcomePopup(uiRoot);
            Assert.That(outcomePopup, Is.Not.Null);
            RectTransform scrim = (RectTransform)outcomePopup.transform.Find("Scrim");
            RectTransform card = (RectTransform)outcomePopup.transform.Find("SafeArea/Card");
            Rect scrimRect = ScreenRect(scrim);
            Rect cardRect = ScreenRect(card);
            Vector2 scrimPoint = new(scrimRect.xMin + 16f, scrimRect.yMin + 16f);
            Assert.That(cardRect.Contains(scrimPoint), Is.False, "The chosen scrim point must lie outside the outcome card.");

            // Act
            yield return IE_ClickScreenPoint(mouse, scrimPoint);

            // Assert
            Assert.That(CurrentOutcomePopup(uiRoot), Is.SameAs(outcomePopup), "The scrim must not dismiss the outcome popup.");
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Null);
            Assert.That(VisibleMarks(), Has.Length.EqualTo(5), "The completed board should be unchanged behind the scrim.");
        }

        [UnityTest]
        public IEnumerator Quit_confirmation_no_resumes_yes_returns_to_menu_and_the_next_start_is_fresh()
        {
            // Arrange
            yield return IE_LoadScene();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
            Transform firstCell = Cell(0, 0);
            Transform secondCell = Cell(0, 1);

            // Act
            yield return IE_StartGame(mouse);
            yield return IE_PressCell(mouse, firstCell);
            Sprite xSprite = MarkAt(firstCell).GetComponent<SpriteRenderer>().sprite;
            yield return IE_PressBack(keyboard);

            // Assert
            ConfirmQuitPopup firstConfirmation = CurrentConfirmation(uiRoot);
            Assert.That(firstConfirmation, Is.Not.Null);

            // Act
            yield return IE_PressCell(mouse, secondCell);

            // Assert
            Assert.That(MarkAt(secondCell), Is.Null, "Board input should remain blocked while quit confirmation is present.");

            // Act
            yield return IE_ClickButton(mouse, NoButton(firstConfirmation));

            // Assert
            Assert.That(CurrentConfirmation(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            // Act
            yield return IE_PressCell(mouse, secondCell);

            // Assert
            Assert.That(MarkAt(secondCell), Is.Not.Null, "Answering No should restore board input.");

            // Act
            yield return IE_PressBack(keyboard);

            // Assert
            ConfirmQuitPopup secondConfirmation = CurrentConfirmation(uiRoot);
            Assert.That(secondConfirmation, Is.Not.Null);

            // Act
            yield return IE_ClickButton(mouse, YesButton(secondConfirmation));

            // Assert
            Assert.That(CurrentConfirmation(uiRoot), Is.Null);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Null);
            Assert.That(Object.FindFirstObjectByType<MainMenuPanel>(), Is.Not.Null);
            Assert.That(VisibleMarks(), Has.Length.EqualTo(2), "Returning to the menu should preserve the abandoned game until the next start.");

            // Act
            yield return IE_StartGame(mouse);

            // Assert
            Assert.That(VisibleMarks(), Is.Empty);
            Assert.That(Object.FindFirstObjectByType<GameplayPanel>(), Is.Not.Null);

            // Act
            yield return IE_PressCell(mouse, firstCell);

            // Assert
            MarkView freshX = MarkAt(firstCell);
            Assert.That(freshX, Is.Not.Null);
            Assert.That(freshX.GetComponent<SpriteRenderer>().sprite, Is.SameAs(xSprite));
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

        private static Rect ScreenRect(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            return new Rect(min, max - min);
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
