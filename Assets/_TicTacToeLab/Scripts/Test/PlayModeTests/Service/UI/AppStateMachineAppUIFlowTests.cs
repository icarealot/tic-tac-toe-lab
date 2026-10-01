#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class AppStateMachineAppUIFlowTests
    {
        private static readonly CellCoordinate[] X_WIN_SEQUENCE =
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(1, 1),
            new(0, 2),
        };

        private GeneratedAppFlowFixture _fixture;

        [SetUp]
        public void CreateGeneratedAppFlow()
        {
            _fixture = new GeneratedAppFlowFixture();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedAppFlow()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [UnityTest]
        public IEnumerator Confirming_quit_closes_the_popup_before_returning_Home()
        {
            // Arrange
            GeneratedConfirmQuitScreen confirmation = null;
            LifecycleProbe gameplayProbe = null;
            LifecycleProbe confirmationProbe = null;
            yield return IE_ArrangeQuitConfirmation(
                (arrangedConfirmation, outgoingGameplay, arrangedConfirmationProbe) =>
                {
                    confirmation = arrangedConfirmation;
                    gameplayProbe = outgoingGameplay;
                    confirmationProbe = arrangedConfirmationProbe;
                });

            // Act
            confirmation.Confirm();

            // Assert
            yield return IE_WaitForCompletedHome(
                "Confirming quit should close the popup before returning to Home.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => gameplayProbe.WasDestroyed && confirmationProbe.WasDestroyed,
                "The quit flow should destroy its outgoing gameplay and popup screens.");
            AssertCompletedHome();
        }

        [UnityTest]
        public IEnumerator Acknowledging_a_terminal_outcome_closes_the_popup_before_returning_Home()
        {
            // Arrange
            GeneratedOutcomeScreen outcome = null;
            LifecycleProbe gameplayProbe = null;
            LifecycleProbe outcomeProbe = null;
            yield return IE_ArrangeTerminalOutcome(
                (arrangedOutcome, outgoingGameplay, arrangedOutcomeProbe) =>
                {
                    outcome = arrangedOutcome;
                    gameplayProbe = outgoingGameplay;
                    outcomeProbe = arrangedOutcomeProbe;
                });

            // Act
            outcome.Acknowledge();

            // Assert
            yield return IE_WaitForCompletedHome(
                "Acknowledging the outcome should close the popup before returning to Home.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => gameplayProbe.WasDestroyed && outcomeProbe.WasDestroyed,
                "The outcome flow should destroy its outgoing gameplay and popup screens.");
            AssertCompletedHome();
        }

        private IEnumerator IE_ArrangeQuitConfirmation(
            Action<GeneratedConfirmQuitScreen, LifecycleProbe, LifecycleProbe> capture)
        {
            yield return IE_WaitForHome("The generated application flow should start on Home.");
            GeneratedHomeScreen home = RequireShown<GeneratedHomeScreen>();
            home.SelectPvp();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => IsShown<GeneratedGameplayScreen>(),
                "Selecting PvP should show the generated gameplay screen.");
            GeneratedGameplayScreen gameplay = RequireShown<GeneratedGameplayScreen>();
            LifecycleProbe gameplayProbe = _fixture.UI.Screens.RequireLatestFor<GeneratedGameplayScreen>();

            gameplay.PressBack();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => IsShown<GeneratedConfirmQuitScreen>(),
                "Pressing Back should show the generated quit confirmation popup.");
            GeneratedConfirmQuitScreen confirmation = RequireShown<GeneratedConfirmQuitScreen>();
            LifecycleProbe confirmationProbe = _fixture.UI.Screens.RequireLatestFor<GeneratedConfirmQuitScreen>();
            capture(confirmation, gameplayProbe, confirmationProbe);
        }

        private IEnumerator IE_ArrangeTerminalOutcome(
            Action<GeneratedOutcomeScreen, LifecycleProbe, LifecycleProbe> capture)
        {
            yield return IE_WaitForHome("The generated application flow should start on Home.");
            GeneratedHomeScreen home = RequireShown<GeneratedHomeScreen>();
            home.SelectPvp();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => IsShown<GeneratedGameplayScreen>(),
                "Selecting PvP should show the generated gameplay screen.");
            LifecycleProbe gameplayProbe = _fixture.UI.Screens.RequireLatestFor<GeneratedGameplayScreen>();

            foreach (CellCoordinate coordinate in X_WIN_SEQUENCE)
            {
                Assert.That(
                    _fixture.BoardPresenter.TryPlaceMark(coordinate),
                    Is.True,
                    "The generated board should accept every placement in the terminal sequence.");
            }

            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => _fixture.DelayScheduler.HasPending,
                "A terminal game should schedule observable outcome presentation.");
            _fixture.DelayScheduler.FirePending();
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => IsShown<GeneratedOutcomeScreen>(),
                "Completing the game should show the generated outcome popup.");
            GeneratedOutcomeScreen outcome = RequireShown<GeneratedOutcomeScreen>();
            LifecycleProbe outcomeProbe = _fixture.UI.Screens.RequireLatestFor<GeneratedOutcomeScreen>();
            capture(outcome, gameplayProbe, outcomeProbe);
        }

        private IEnumerator IE_WaitForHome(string failureMessage)
        {
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => IsShown<GeneratedHomeScreen>(),
                failureMessage);
        }

        private IEnumerator IE_WaitForCompletedHome(string failureMessage)
        {
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => IsShown<GeneratedHomeScreen>()
                    && !IsShown<GeneratedGameplayScreen>()
                    && !HasActivePopup(),
                failureMessage);
        }

        private void AssertCompletedHome()
        {
            Assert.That(IsShown<GeneratedHomeScreen>(), Is.True, "The completed flow should leave Home active.");
            Assert.That(IsShown<GeneratedGameplayScreen>(), Is.False, "The completed flow should leave gameplay inactive.");
            Assert.That(HasActivePopup(), Is.False, "The completed flow should leave no popup active.");
        }

        private bool HasActivePopup()
        {
            return IsShown<GeneratedConfirmQuitScreen>() || IsShown<GeneratedOutcomeScreen>();
        }

        private bool IsShown<T>() where T : Component
        {
            LifecycleProbe probe = _fixture.UI.Screens.LatestFor<T>();
            return probe != null && probe.IsShown;
        }

        private T RequireShown<T>() where T : Component
        {
            LifecycleProbe probe = _fixture.UI.Screens.RequireLatestFor<T>();
            Assert.That(probe.IsShown, Is.True, $"The generated flow should show an active {typeof(T).Name}.");
            return probe.GetComponent<T>();
        }
    }
}
#endif
