using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class UIServiceTests
    {
        private FakeUIFactoryService _uiFactory;
        private UIService _uiService;

        [SetUp]
        public void SetUp()
        {
            _uiFactory = new FakeUIFactoryService();
            _uiService = new UIService(_uiFactory);
        }

        [Test]
        public void Showing_a_panel_obtains_it_from_the_ui_factory_and_makes_it_visible()
        {
            _uiService.ShowPanel<IGameplayPanel>();

            Assert.That(_uiFactory.Panels, Has.Count.EqualTo(1));
            Assert.That(_uiFactory.Panels[0].IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_panel_hides_the_first_without_returning_it()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel firstPanel = _uiFactory.Panels[0];

            _uiService.ShowPanel<IGameplayPanel>();

            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(_uiFactory.ReturnedWindows, Is.Empty);
        }

        [Test]
        public void The_configure_step_runs_before_the_panel_becomes_visible()
        {
            bool? wasVisibleDuringConfigure = null;

            _uiService.ShowPanel<IGameplayPanel>(panel => wasVisibleDuringConfigure = panel.IsVisible);

            Assert.That(wasVisibleDuringConfigure, Is.False);
        }

        [Test]
        public void Closing_the_top_panel_returns_it_and_reveals_the_one_beneath_with_its_configuration_intact()
        {
            FakeBoardSession boardSession = new();
            _uiService.ShowPanel<IGameplayPanel>(panel => panel.Setup(boardSession));
            FakeGameplayPanel firstPanel = _uiFactory.Panels[0];
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel secondPanel = _uiFactory.Panels[1];

            bool wasClosed = _uiService.TryClosePanel();

            Assert.That(wasClosed, Is.True);
            Assert.That(_uiFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
            Assert.That(firstPanel.ConfiguredBoardSession, Is.SameAs(boardSession));
        }

        [Test]
        public void Closing_a_panel_when_the_stack_is_empty_reports_that_nothing_was_closed()
        {
            Assert.That(_uiService.TryClosePanel(), Is.False);
        }

        [Test]
        public void Showing_a_popup_obtains_it_from_the_ui_factory_and_makes_it_visible()
        {
            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(_uiFactory.Popups, Has.Count.EqualTo(1));
            Assert.That(_uiFactory.Popups[0].IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_popup_leaves_the_panel_beneath_it_visible()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _uiFactory.Panels[0];

            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_popup_hides_the_first_but_never_the_panel_behind_them()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _uiFactory.Panels[0];
            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup firstPopup = _uiFactory.Popups[0];

            _uiService.ShowPopup<IConfirmQuitPopup>();

            Assert.That(firstPopup.IsVisible, Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_uiFactory.ReturnedWindows, Is.Empty);
        }

        [Test]
        public void Closing_the_top_popup_returns_it_and_reveals_the_one_beneath()
        {
            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup firstPopup = _uiFactory.Popups[0];
            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup secondPopup = _uiFactory.Popups[1];

            bool wasClosed = _uiService.TryClosePopup();

            Assert.That(wasClosed, Is.True);
            Assert.That(_uiFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPopup }));
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
            FakeGameplayPanel panel = _uiFactory.Panels[0];
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
            FakeGameplayPanel panel = _uiFactory.Panels[0];
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
