#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class AppUITests
    {
        private GeneratedAppUIFixture _fixture;
        private AppUI _sut;
        private BoardPresenter _boardPresenter;

        [SetUp]
        public void CreateGeneratedAppUI()
        {
            _fixture = new GeneratedAppUIFixture();
            _sut = _fixture.CreateAppUI();
            _boardPresenter = new BoardPresenterHarness().Presenter;
        }

        [UnityTearDown]
        public IEnumerator DestroyGeneratedAppUI()
        {
            yield return _fixture.IE_DestroyAll();
        }

        [UnityTest]
        public IEnumerator Showing_gameplay_replaces_and_destroys_the_previous_screen()
        {
            // Arrange
            _sut.ShowHome(() => { });
            LifecycleProbe replacedScreen = _fixture.Screens.RequireLatestFor<HomeScreen>();

            // Act
            _sut.ShowGameplay(_boardPresenter, () => { });

            // Assert
            Assert.That(_fixture.Screens.RequireLatestFor<GameplayScreen>().IsShown, Is.True, "Showing gameplay should present the current screen.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => replacedScreen.WasDestroyed,
                "Showing gameplay should destroy the previous screen instance.");
        }

        [UnityTest]
        public IEnumerator Showing_a_popup_preserves_gameplay_and_replaces_the_previous_popup()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            LifecycleProbe gameplay = _fixture.Screens.RequireLatestFor<GameplayScreen>();

            _sut.ShowQuitConfirmation(() => { }, () => { });
            LifecycleProbe replacedPopup = _fixture.Screens.RequireLatestFor<ConfirmQuitScreen>();

            // Act
            _sut.ShowOutcome(Outcome.XWin, () => { });

            // Assert
            Assert.That(_fixture.Screens.RequireLatestFor<OutcomeScreen>().IsShown, Is.True, "Showing the outcome should present the current popup.");
            Assert.That(gameplay.IsShown, Is.True, "Showing a popup should keep the gameplay screen shown beneath it.");
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => replacedPopup.WasDestroyed,
                "Replacing the popup should destroy the previous popup instance.");
        }

        [UnityTest]
        public IEnumerator Closing_quit_confirmation_destroys_its_popup_and_preserves_gameplay()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            LifecycleProbe gameplay = _fixture.Screens.RequireLatestFor<GameplayScreen>();

            _sut.ShowQuitConfirmation(() => { }, () => { });
            LifecycleProbe popup = _fixture.Screens.RequireLatestFor<ConfirmQuitScreen>();

            // Act
            _sut.CloseQuitConfirmation();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => popup.WasDestroyed,
                "Closing quit confirmation should destroy its popup instance.");
            Assert.That(gameplay.IsShown, Is.True, "Closing quit confirmation should keep the current screen shown.");
        }

        [UnityTest]
        public IEnumerator Closing_the_popup_destroys_it_and_preserves_gameplay()
        {
            // Arrange
            _sut.ShowGameplay(_boardPresenter, () => { });
            LifecycleProbe gameplay = _fixture.Screens.RequireLatestFor<GameplayScreen>();

            _sut.ShowOutcome(Outcome.Draw, () => { });
            LifecycleProbe popup = _fixture.Screens.RequireLatestFor<OutcomeScreen>();

            // Act
            _sut.ClosePopup();

            // Assert
            yield return PlayModeWait.IE_WaitUntilOrFail(
                () => popup.WasDestroyed,
                "Closing the popup should destroy its popup instance.");
            Assert.That(gameplay.IsShown, Is.True, "Closing the popup should keep the current screen shown.");
        }
    }
}
#endif
