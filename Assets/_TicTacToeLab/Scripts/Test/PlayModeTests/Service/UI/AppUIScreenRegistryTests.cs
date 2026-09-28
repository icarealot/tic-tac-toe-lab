#if UNITY_EDITOR
using System;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class AppUIScreenRegistryTests
    {
        private GeneratedAppUIFixture _fixture;

        [SetUp]
        public void CreateGeneratedAppUIFixture()
        {
            _fixture = new GeneratedAppUIFixture();
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedAppUIFixture()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [Test]
        public void Showing_a_registered_base_screen_presents_it_on_the_base_layer_and_invokes_the_configuration_callback_once()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            AppUI sut = CreateActiveAppUI(new ScreenRegistration(homePrefab, ScreenLayer.Base));
            IHomeScreen configuredScreen = null;
            int configureCount = 0;

            // Act
            sut.Show<IHomeScreen>(screen =>
            {
                configuredScreen = screen;
                configureCount++;
            });

            // Assert
            LifecycleProbe presented = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();
            Assert.That(presented.IsShown, Is.True, "Showing a registered base screen should present it.");
            Assert.That(
                presented.transform.parent,
                Is.SameAs(_fixture.PanelLayer),
                "A base registration should instantiate under the base layer.");
            Assert.That(
                configuredScreen,
                Is.SameAs(presented.GetComponent<GeneratedHomeScreen>()),
                "The configuration callback should receive the shown role instance.");
            Assert.That(configureCount, Is.EqualTo(1), "The configuration callback should be invoked exactly once.");
        }

        [Test]
        public void Showing_a_registered_screen_without_a_configuration_callback_presents_it()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            AppUI sut = CreateActiveAppUI(new ScreenRegistration(homePrefab, ScreenLayer.Base));

            // Act
            sut.Show<IHomeScreen>();

            // Assert
            Assert.That(
                _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>().IsShown,
                Is.True,
                "Showing a registered screen without a configuration callback should present it.");
        }

        [UnityTest]
        public IEnumerator Showing_another_base_screen_deactivates_the_outgoing_screen_immediately_and_destroys_it_eventually()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedGameplayScreen gameplayPrefab = _fixture.CreateScreenTemplate<GeneratedGameplayScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(gameplayPrefab, ScreenLayer.Base));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            LifecycleProbe outgoing = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();
            Assert.That(outgoing.IsShown, Is.True, "The first base screen should be presented before it is replaced.");

            // Act
            sut.Show<IGameplayScreen>(screen => screen.Setup(null, () => { }));

            // Assert
            Assert.That(outgoing.IsShown, Is.False, "Replacing a base screen should deactivate the outgoing screen immediately.");
            Assert.That(
                _fixture.Windows.RequireLatestFor<GeneratedGameplayScreen>().IsShown,
                Is.True,
                "Replacing a base screen should present the incoming screen.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => outgoing.WasDestroyed,
                "Replacing a base screen should destroy the outgoing screen.");
        }

        [UnityTest]
        public IEnumerator Showing_a_popup_preserves_the_base_screen_and_replacing_the_popup_deactivates_and_destroys_the_outgoing_popup()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedConfirmQuitScreen quitConfirmationPrefab = _fixture.CreateScreenTemplate<GeneratedConfirmQuitScreen>();
            GeneratedOutcomeScreen outcomePrefab = _fixture.CreateScreenTemplate<GeneratedOutcomeScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(quitConfirmationPrefab, ScreenLayer.Popup),
                new ScreenRegistration(outcomePrefab, ScreenLayer.Popup));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            LifecycleProbe baseScreen = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();

            // Act
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => { }, () => { }));

            // Assert
            LifecycleProbe outgoingPopup = _fixture.Windows.RequireLatestFor<GeneratedConfirmQuitScreen>();
            Assert.That(outgoingPopup.IsShown, Is.True, "Showing a popup should present it.");
            Assert.That(
                outgoingPopup.transform.parent,
                Is.SameAs(_fixture.PopupLayer),
                "A popup registration should instantiate under the popup layer.");
            Assert.That(baseScreen.IsShown, Is.True, "Showing a popup should preserve the base screen.");

            // Act
            sut.Show<IOutcomeScreen>(screen => screen.Setup(Outcome.XWin, () => { }));

            // Assert
            Assert.That(
                _fixture.Windows.RequireLatestFor<GeneratedOutcomeScreen>().IsShown,
                Is.True,
                "Replacing a popup should present the incoming popup.");
            Assert.That(baseScreen.IsShown, Is.True, "Replacing a popup should preserve the base screen.");
            Assert.That(outgoingPopup.IsShown, Is.False, "Replacing a popup should deactivate the outgoing popup immediately.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => outgoingPopup.WasDestroyed,
                "Replacing a popup should destroy the outgoing popup.");
        }

        [Test]
        public void Showing_a_popup_without_an_active_base_screen_is_rejected()
        {
            // Arrange
            GeneratedConfirmQuitScreen quitConfirmationPrefab = _fixture.CreateScreenTemplate<GeneratedConfirmQuitScreen>();
            AppUI sut = CreateActiveAppUI(new ScreenRegistration(quitConfirmationPrefab, ScreenLayer.Popup));

            // Act and Assert
            Assert.That(
                () => sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => { }, () => { })),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains(nameof(IConfirmQuitScreen)),
                "The rejected popup failure should identify the popup role.");
            Assert.That(
                _fixture.Windows.LatestFor<GeneratedConfirmQuitScreen>(),
                Is.Null,
                "A rejected popup should not be instantiated.");
        }

        [UnityTest]
        public IEnumerator Showing_a_base_screen_while_a_popup_is_active_is_rejected_and_preserves_both_screens()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedGameplayScreen gameplayPrefab = _fixture.CreateScreenTemplate<GeneratedGameplayScreen>();
            GeneratedConfirmQuitScreen quitConfirmationPrefab = _fixture.CreateScreenTemplate<GeneratedConfirmQuitScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(gameplayPrefab, ScreenLayer.Base),
                new ScreenRegistration(quitConfirmationPrefab, ScreenLayer.Popup));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => { }, () => { }));
            LifecycleProbe baseScreen = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();
            LifecycleProbe popup = _fixture.Windows.RequireLatestFor<GeneratedConfirmQuitScreen>();

            // Act and Assert
            Assert.That(
                () => sut.Show<IGameplayScreen>(screen => screen.Setup(null, () => { })),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains(nameof(IGameplayScreen)),
                "The rejected base failure should identify the base role.");
            Assert.That(
                _fixture.Windows.LatestFor<GeneratedGameplayScreen>(),
                Is.Null,
                "A rejected base screen should not be instantiated.");
            Assert.That(baseScreen.IsShown, Is.True, "Rejecting base navigation should keep the active base screen shown.");
            Assert.That(popup.IsShown, Is.True, "Rejecting base navigation should keep the active popup shown.");
            yield return null;
            Assert.That(baseScreen.WasDestroyed, Is.False, "Rejecting base navigation should not destroy the active base screen.");
            Assert.That(popup.WasDestroyed, Is.False, "Rejecting base navigation should not destroy the active popup.");
        }

        [Test]
        public void Closing_the_active_base_screen_while_a_popup_is_active_is_rejected()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedConfirmQuitScreen quitConfirmationPrefab = _fixture.CreateScreenTemplate<GeneratedConfirmQuitScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(quitConfirmationPrefab, ScreenLayer.Popup));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => { }, () => { }));
            LifecycleProbe baseScreen = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();

            // Act and Assert
            Assert.That(
                () => sut.Close<IHomeScreen>(),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains(nameof(IHomeScreen)),
                "The rejected base close failure should identify the base role.");
            Assert.That(baseScreen.IsShown, Is.True, "Rejecting a base close should keep the base screen shown.");
        }

        [UnityTest]
        public IEnumerator Closing_the_active_popup_deactivates_it_immediately_and_destroys_it()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedOutcomeScreen outcomePrefab = _fixture.CreateScreenTemplate<GeneratedOutcomeScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(outcomePrefab, ScreenLayer.Popup));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            sut.Show<IOutcomeScreen>(screen => screen.Setup(Outcome.XWin, () => { }));
            LifecycleProbe baseScreen = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();
            LifecycleProbe popup = _fixture.Windows.RequireLatestFor<GeneratedOutcomeScreen>();

            // Act
            sut.Close<IOutcomeScreen>();

            // Assert
            Assert.That(popup.IsShown, Is.False, "Closing the active popup should deactivate it immediately.");
            Assert.That(baseScreen.IsShown, Is.True, "Closing a popup should preserve the base screen.");
            yield return PlayModeWait.IE_WaitUntilOrFail(() => popup.WasDestroyed, "Closing the active popup should destroy it.");
            Assert.That(
                () => sut.Close<IOutcomeScreen>(),
                Throws.Nothing,
                "Closing an already closed popup role should be safe.");
        }

        [UnityTest]
        public IEnumerator Closing_the_active_base_screen_deactivates_it_immediately_and_destroys_it()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            AppUI sut = CreateActiveAppUI(new ScreenRegistration(homePrefab, ScreenLayer.Base));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            LifecycleProbe baseScreen = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();

            // Act
            sut.Close<IHomeScreen>();

            // Assert
            Assert.That(baseScreen.IsShown, Is.False, "Closing the active base screen should deactivate it immediately.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => baseScreen.WasDestroyed,
                "Closing the active base screen should destroy it.");
        }

        [Test]
        public void Closing_a_registered_role_that_is_not_shown_does_nothing()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedOutcomeScreen outcomePrefab = _fixture.CreateScreenTemplate<GeneratedOutcomeScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(outcomePrefab, ScreenLayer.Popup));

            // Act
            sut.Close<IHomeScreen>();
            sut.Close<IOutcomeScreen>();

            // Assert
            Assert.That(_fixture.Windows.LatestFor<GeneratedHomeScreen>(), Is.Null, "Closing an inactive role should not create a screen.");
            Assert.That(_fixture.Windows.LatestFor<GeneratedOutcomeScreen>(), Is.Null, "Closing an inactive role should not create a screen.");
        }

        [UnityTest]
        public IEnumerator Closing_a_replaced_popup_role_leaves_the_current_popup_untouched()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedConfirmQuitScreen quitConfirmationPrefab = _fixture.CreateScreenTemplate<GeneratedConfirmQuitScreen>();
            GeneratedOutcomeScreen outcomePrefab = _fixture.CreateScreenTemplate<GeneratedOutcomeScreen>();
            AppUI sut = CreateActiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(quitConfirmationPrefab, ScreenLayer.Popup),
                new ScreenRegistration(outcomePrefab, ScreenLayer.Popup));
            sut.Show<IHomeScreen>(screen => screen.Setup(() => { }));
            sut.Show<IConfirmQuitScreen>(screen => screen.Setup(() => { }, () => { }));
            sut.Show<IOutcomeScreen>(screen => screen.Setup(Outcome.Draw, () => { }));
            LifecycleProbe currentPopup = _fixture.Windows.RequireLatestFor<GeneratedOutcomeScreen>();

            // Act
            sut.Close<IConfirmQuitScreen>();

            // Assert
            Assert.That(currentPopup.IsShown, Is.True, "Closing a replaced popup role should not close the current popup.");
            yield return null;
            Assert.That(currentPopup.WasDestroyed, Is.False, "Closing a replaced popup role should not destroy the current popup.");
        }

        [Test]
        public void A_registration_without_a_prefab_fails_initialization()
        {
            // Arrange
            AppUI sut = _fixture.CreateInactiveAppUI(new ScreenRegistration(null, ScreenLayer.Base));

            // Act
            LogAssert.Expect(LogType.Exception, new Regex("registration must reference a screen component prefab"));
            sut.gameObject.SetActive(true);
        }

        [Test]
        public void A_registration_without_a_screen_role_fails_initialization()
        {
            // Arrange
            GeneratedRolelessScreen rolelessPrefab = _fixture.CreateScreenTemplate<GeneratedRolelessScreen>();
            AppUI sut = _fixture.CreateInactiveAppUI(new ScreenRegistration(rolelessPrefab, ScreenLayer.Base));

            // Act
            LogAssert.Expect(
                LogType.Exception,
                new Regex("GeneratedRolelessScreen implements no screen role interface"));
            sut.gameObject.SetActive(true);
        }

        [Test]
        public void A_registration_with_multiple_screen_roles_fails_initialization()
        {
            // Arrange
            GeneratedMultiRoleScreen multiRolePrefab = _fixture.CreateScreenTemplate<GeneratedMultiRoleScreen>();
            AppUI sut = _fixture.CreateInactiveAppUI(new ScreenRegistration(multiRolePrefab, ScreenLayer.Base));

            // Act
            LogAssert.Expect(
                LogType.Exception,
                new Regex("GeneratedMultiRoleScreen implements more than one screen role interface"));
            sut.gameObject.SetActive(true);
        }

        [Test]
        public void A_role_registered_twice_fails_initialization()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            AppUI sut = _fixture.CreateInactiveAppUI(
                new ScreenRegistration(homePrefab, ScreenLayer.Base),
                new ScreenRegistration(homePrefab, ScreenLayer.Popup));

            // Act
            LogAssert.Expect(LogType.Exception, new Regex("IHomeScreen is registered more than once"));
            sut.gameObject.SetActive(true);
        }

        [Test]
        public void Showing_an_unregistered_role_fails_and_identifies_the_role()
        {
            // Arrange
            AppUI sut = CreateActiveAppUI();

            // Act and Assert
            Assert.That(
                () => sut.Show<IHomeScreen>(),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains(nameof(IHomeScreen)),
                "An unregistered show failure should identify the role.");
        }

        [Test]
        public void Closing_an_unregistered_role_fails_and_identifies_the_role()
        {
            // Arrange
            AppUI sut = CreateActiveAppUI();

            // Act and Assert
            Assert.That(
                () => sut.Close<IHomeScreen>(),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains(nameof(IHomeScreen)),
                "An unregistered close failure should identify the role.");
        }

        [UnityTest]
        public IEnumerator A_configuration_callback_failure_propagates_and_does_not_leak_the_partially_created_screen()
        {
            // Arrange
            GeneratedHomeScreen homePrefab = _fixture.CreateScreenTemplate<GeneratedHomeScreen>();
            AppUI sut = CreateActiveAppUI(new ScreenRegistration(homePrefab, ScreenLayer.Base));
            LifecycleProbe partiallyCreated = null;

            // Act and Assert
            Assert.That(
                () => sut.Show<IHomeScreen>(screen =>
                {
                    partiallyCreated = _fixture.Windows.RequireLatestFor<GeneratedHomeScreen>();
                    throw new InvalidOperationException("Configuration failed.");
                }),
                Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Configuration failed."),
                "The configuration failure should propagate.");
            Assert.That(partiallyCreated, Is.Not.Null, "The partially created screen should exist when configuration fails.");
            Assert.That(partiallyCreated.IsShown, Is.False, "A partially created screen should be deactivated.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => partiallyCreated.WasDestroyed,
                "A partially created screen should be destroyed.");
        }

        private AppUI CreateActiveAppUI(params ScreenRegistration[] registrations)
        {
            AppUI appUI = _fixture.CreateInactiveAppUI(registrations);
            appUI.gameObject.SetActive(true);
            return appUI;
        }
    }
}
#endif
