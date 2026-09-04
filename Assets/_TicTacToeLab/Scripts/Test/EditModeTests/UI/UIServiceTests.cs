using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class UIServiceTests
    {
        private FakeUIRoot _fakeUIRoot;
        private FakeFactoryService _fakeFactoryService;
        private UIService _uiService;

        [SetUp]
        public void SetUp()
        {
            _fakeUIRoot = new FakeUIRoot();
            _fakeFactoryService = new FakeFactoryService();
            _uiService = new UIService(_fakeFactoryService, _fakeUIRoot, new FakeCoroutineService());
        }

        [Test]
        public void Showing_a_panel_parents_it_under_the_panel_layer_and_makes_it_visible()
        {
            GameplayPanel shownPanel = null;
            _uiService.ShowPanel<GameplayPanel>(panel => shownPanel = panel);

            Assert.That(shownPanel.transform.parent, Is.EqualTo(_fakeUIRoot.PanelLayer));
            Assert.That(shownPanel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_panel_hides_the_first_without_destroying_it()
        {
            GameplayPanel firstPanel = null;
            _uiService.ShowPanel<GameplayPanel>(panel => firstPanel = panel);

            _uiService.ShowPanel<GameplayPanel>();

            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(_fakeFactoryService.ReturnedInstances, Is.Empty);
        }

        [Test]
        public void The_configure_step_runs_before_the_panel_becomes_visible()
        {
            bool? wasVisibleDuringConfigure = null;

            _uiService.ShowPanel<GameplayPanel>(panel => wasVisibleDuringConfigure = panel.IsVisible);

            Assert.That(wasVisibleDuringConfigure, Is.False);
        }

        [Test]
        public void Closing_the_top_panel_destroys_it_through_the_factory_and_reveals_the_one_beneath_with_its_configuration_intact()
        {
            GameplayPanel firstPanel = null;
            _uiService.ShowPanel<GameplayPanel>(panel => firstPanel = panel);
            GameplayPanel secondPanel = null;
            _uiService.ShowPanel<GameplayPanel>(panel => secondPanel = panel);

            bool wasClosed = _uiService.TryClosePanel();

            Assert.That(wasClosed, Is.True);
            Assert.That(_fakeFactoryService.ReturnedInstances, Is.EqualTo(new Component[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
        }

        [Test]
        public void Closing_a_panel_when_the_stack_is_empty_reports_that_nothing_was_closed_and_does_not_throw()
        {
            bool wasClosed = _uiService.TryClosePanel();

            Assert.That(wasClosed, Is.False);
        }

        [Test]
        public void Showing_a_popup_parents_it_under_the_popup_layer_and_makes_it_visible()
        {
            ConfirmQuitPopup shownPopup = null;
            _uiService.ShowPopup<ConfirmQuitPopup>(popup => shownPopup = popup);

            Assert.That(shownPopup.transform.parent, Is.EqualTo(_fakeUIRoot.PopupLayer));
            Assert.That(shownPopup.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_popup_leaves_the_panel_beneath_it_visible()
        {
            GameplayPanel panel = null;
            _uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);

            _uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_popup_hides_the_first_but_never_the_panel_behind_them()
        {
            GameplayPanel panel = null;
            _uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);
            ConfirmQuitPopup firstPopup = null;
            _uiService.ShowPopup<ConfirmQuitPopup>(popup => firstPopup = popup);

            _uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(firstPopup.IsVisible, Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_fakeFactoryService.ReturnedInstances, Is.Empty);
        }

        [Test]
        public void Closing_the_top_popup_destroys_it_through_the_factory_and_reveals_the_one_beneath()
        {
            ConfirmQuitPopup firstPopup = null;
            _uiService.ShowPopup<ConfirmQuitPopup>(popup => firstPopup = popup);
            ConfirmQuitPopup secondPopup = null;
            _uiService.ShowPopup<ConfirmQuitPopup>(popup => secondPopup = popup);

            bool wasClosed = _uiService.TryClosePopup();

            Assert.That(wasClosed, Is.True);
            Assert.That(_fakeFactoryService.ReturnedInstances, Is.EqualTo(new Component[] { secondPopup }));
            Assert.That(firstPopup.IsVisible, Is.True);
        }

        [Test]
        public void Closing_a_popup_when_the_stack_is_empty_reports_that_nothing_was_closed_and_does_not_throw()
        {
            bool wasClosed = _uiService.TryClosePopup();

            Assert.That(wasClosed, Is.False);
        }

        [Test]
        public void The_popup_stack_is_exhausted_before_any_panel_can_be_closed()
        {
            GameplayPanel panel = null;
            _uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);
            _uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(_uiService.TryClosePopup(), Is.True);
            Assert.That(_uiService.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void Closing_every_popup_at_once_empties_the_popup_stack_and_leaves_the_panel_stack_untouched()
        {
            GameplayPanel panel = null;
            _uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);
            _uiService.ShowPopup<ConfirmQuitPopup>();
            _uiService.ShowPopup<ConfirmQuitPopup>();

            _uiService.CloseAllPopups();

            Assert.That(_uiService.HasPopup, Is.False);
            Assert.That(_uiService.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void Asking_whether_a_popup_is_up_agrees_with_the_popup_stack()
        {
            _uiService.ShowPanel<GameplayPanel>();

            Assert.That(_uiService.HasPopup, Is.False);

            _uiService.ShowPopup<ConfirmQuitPopup>();
            Assert.That(_uiService.HasPopup, Is.True);

            _uiService.ShowPopup<ConfirmQuitPopup>();
            Assert.That(_uiService.HasPopup, Is.True);

            _ = _uiService.TryClosePopup();
            Assert.That(_uiService.HasPopup, Is.True);

            _ = _uiService.TryClosePopup();
            Assert.That(_uiService.HasPopup, Is.False);
        }

        [Test]
        public void Showing_a_panel_while_a_popup_is_up_throws()
        {
            _uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(() => _uiService.ShowPanel<GameplayPanel>(), Throws.InstanceOf<InvalidOperationException>());
        }
    }
}
