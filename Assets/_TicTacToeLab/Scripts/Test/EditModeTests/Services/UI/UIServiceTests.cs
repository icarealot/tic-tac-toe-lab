using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class UIServiceTests
    {
        private FakeFactoryService _fakeFactory;
        private FakeCoroutineService _fakeCoroutineService;
        private UIService _uiService;

        [SetUp]
        public void SetUp()
        {
            _fakeFactory = new FakeFactoryService();
            _fakeCoroutineService = new FakeCoroutineService();
            _uiService = new UIService(_fakeFactory, new FakeUIRoot(), _fakeCoroutineService);
        }

        [Test]
        public void Showing_a_panel_requests_its_panel_role_through_the_factory_and_makes_it_visible()
        {
            _uiService.ShowPanel<IGameplayPanel>();

            Assert.That(_fakeFactory.Panels, Has.Count.EqualTo(1));
            Assert.That(_fakeFactory.Panels[0].IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_panel_hides_the_first_without_returning_it()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel firstPanel = _fakeFactory.Panels[0];

            _uiService.ShowPanel<IGameplayPanel>();

            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(_fakeFactory.ReturnedWindows, Is.Empty);
        }

        [Test]
        public void A_new_window_is_constructed_before_the_caller_configuration_runs()
        {
            int constructionCallsBeforeConfigure = 0;

            _uiService.ShowPanel<IGameplayPanel>(panel => constructionCallsBeforeConfigure = _fakeFactory.Panels[0].ConstructionServices.Count);

            Assert.That(constructionCallsBeforeConfigure, Is.EqualTo(1));
        }

        [Test]
        public void A_new_window_is_constructed_with_the_coroutine_service_the_ui_service_received()
        {
            _uiService.ShowPanel<IGameplayPanel>();

            FakeGameplayPanel panel = _fakeFactory.Panels[0];
            Assert.That(panel.ConstructionServices, Is.EqualTo(new ICoroutineService[] { _fakeCoroutineService }));
            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void The_configure_step_runs_before_the_panel_becomes_visible()
        {
            bool? wasVisibleDuringConfigure = null;

            _uiService.ShowPanel<IGameplayPanel>(panel => wasVisibleDuringConfigure = panel.IsVisible);

            Assert.That(wasVisibleDuringConfigure, Is.False);
        }

        [Test]
        public void Closing_the_top_panel_returns_it_through_the_factory_and_reveals_the_one_beneath_with_its_configuration_intact()
        {
            FakeBoardSession boardSession = new();
            _uiService.ShowPanel<IGameplayPanel>(panel => panel.Setup(boardSession, onBack: null));
            FakeGameplayPanel firstPanel = _fakeFactory.Panels[0];
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel secondPanel = _fakeFactory.Panels[1];

            bool wasClosed = _uiService.TryClosePanel();

            Assert.That(wasClosed, Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
            Assert.That(firstPanel.ConfiguredBoardSession, Is.SameAs(boardSession));
        }

        [Test]
        public void Closing_a_panel_when_the_stack_is_empty_reports_that_nothing_was_closed()
        {
            Assert.That(_uiService.TryClosePanel(), Is.False);
        }

        [Test]
        public void Showing_a_popup_requests_its_popup_role_through_the_factory_and_makes_it_visible()
        {
            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(_fakeFactory.Popups, Has.Count.EqualTo(1));
            Assert.That(_fakeFactory.Popups[0].IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_popup_leaves_the_panel_beneath_it_visible()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _fakeFactory.Panels[0];

            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_popup_hides_the_first_but_never_the_panel_behind_them()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _fakeFactory.Panels[0];
            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup firstPopup = _fakeFactory.Popups[0];

            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(firstPopup.IsVisible, Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Is.Empty);
        }

        [Test]
        public void Closing_the_top_popup_returns_it_through_the_factory_and_reveals_the_one_beneath()
        {
            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup firstPopup = _fakeFactory.Popups[0];
            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup secondPopup = _fakeFactory.Popups[1];

            bool wasClosed = _uiService.TryClosePopup();

            Assert.That(wasClosed, Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPopup }));
            Assert.That(firstPopup.IsVisible, Is.True);
        }

        [Test]
        public void Closing_a_popup_when_the_stack_is_empty_reports_that_nothing_was_closed()
        {
            Assert.That(_uiService.TryClosePopup(), Is.False);
        }

        [Test]
        public void The_popup_stack_is_exhausted_before_the_panel_stack_is_touched()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _fakeFactory.Panels[0];
            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(_uiService.TryClosePopup(), Is.True);
            Assert.That(_uiService.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void Closing_every_popup_empties_the_popup_stack_and_leaves_the_panel_stack_untouched()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _fakeFactory.Panels[0];
            _uiService.ShowPopup<IConfirmQuitPopup>();
            _uiService.ShowPopup<IConfirmQuitPopup>();

            _uiService.CloseAllPopups();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(_uiService.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void Asking_whether_a_popup_is_up_agrees_with_the_popup_stack()
        {
            Assert.That(_uiService.HasPopup, Is.False);

            _uiService.ShowPopup<IConfirmQuitPopup>();
            Assert.That(_uiService.HasPopup, Is.True);

            _uiService.ShowPopup<IConfirmQuitPopup>();
            Assert.That(_uiService.HasPopup, Is.True);

            _ = _uiService.TryClosePopup();
            Assert.That(_uiService.HasPopup, Is.True);

            _ = _uiService.TryClosePopup();
            Assert.That(_uiService.HasPopup, Is.False);
        }

        [Test]
        public void Showing_a_panel_while_a_popup_is_up_throws()
        {
            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(() => _uiService.ShowPanel<IGameplayPanel>(), Throws.InstanceOf<InvalidOperationException>());
        }
    }
}
