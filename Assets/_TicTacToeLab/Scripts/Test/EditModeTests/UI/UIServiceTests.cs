using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class UIServiceTests
    {
        [Test]
        public void Showing_a_panel_parents_it_under_the_panel_layer_and_makes_it_visible()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());

            GameplayPanel shownPanel = null;
            uiService.ShowPanel<GameplayPanel>(panel => shownPanel = panel);

            Assert.That(shownPanel.transform.parent, Is.EqualTo(uiRoot.PanelLayer));
            Assert.That(shownPanel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_panel_hides_the_first_without_destroying_it()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            GameplayPanel firstPanel = null;
            uiService.ShowPanel<GameplayPanel>(panel => firstPanel = panel);

            uiService.ShowPanel<GameplayPanel>();

            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(fakeFactoryService.ReturnedInstances, Is.Empty);
        }

        [Test]
        public void The_configure_step_runs_before_the_panel_becomes_visible()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            bool? wasVisibleDuringConfigure = null;

            uiService.ShowPanel<GameplayPanel>(panel => wasVisibleDuringConfigure = panel.IsVisible);

            Assert.That(wasVisibleDuringConfigure, Is.False);
        }

        [Test]
        public void Closing_the_top_panel_destroys_it_through_the_factory_and_reveals_the_one_beneath_with_its_configuration_intact()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            GameplayPanel firstPanel = null;
            uiService.ShowPanel<GameplayPanel>(panel => firstPanel = panel);
            GameplayPanel secondPanel = null;
            uiService.ShowPanel<GameplayPanel>(panel => secondPanel = panel);

            bool wasClosed = uiService.TryClosePanel();

            Assert.That(wasClosed, Is.True);
            Assert.That(fakeFactoryService.ReturnedInstances, Is.EqualTo(new Component[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
        }

        [Test]
        public void Closing_a_panel_when_the_stack_is_empty_reports_that_nothing_was_closed_and_does_not_throw()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());

            bool wasClosed = uiService.TryClosePanel();

            Assert.That(wasClosed, Is.False);
        }

        [Test]
        public void Showing_a_popup_parents_it_under_the_popup_layer_and_makes_it_visible()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());

            ConfirmQuitPopup shownPopup = null;
            uiService.ShowPopup<ConfirmQuitPopup>(popup => shownPopup = popup);

            Assert.That(shownPopup.transform.parent, Is.EqualTo(uiRoot.PopupLayer));
            Assert.That(shownPopup.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_popup_leaves_the_panel_beneath_it_visible()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            GameplayPanel panel = null;
            uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);

            uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_second_popup_hides_the_first_but_never_the_panel_behind_them()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            GameplayPanel panel = null;
            uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);
            ConfirmQuitPopup firstPopup = null;
            uiService.ShowPopup<ConfirmQuitPopup>(popup => firstPopup = popup);

            uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(firstPopup.IsVisible, Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(fakeFactoryService.ReturnedInstances, Is.Empty);
        }

        [Test]
        public void Closing_the_top_popup_destroys_it_through_the_factory_and_reveals_the_one_beneath()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            ConfirmQuitPopup firstPopup = null;
            uiService.ShowPopup<ConfirmQuitPopup>(popup => firstPopup = popup);
            ConfirmQuitPopup secondPopup = null;
            uiService.ShowPopup<ConfirmQuitPopup>(popup => secondPopup = popup);

            bool wasClosed = uiService.TryClosePopup();

            Assert.That(wasClosed, Is.True);
            Assert.That(fakeFactoryService.ReturnedInstances, Is.EqualTo(new Component[] { secondPopup }));
            Assert.That(firstPopup.IsVisible, Is.True);
        }

        [Test]
        public void Closing_a_popup_when_the_stack_is_empty_reports_that_nothing_was_closed_and_does_not_throw()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());

            bool wasClosed = uiService.TryClosePopup();

            Assert.That(wasClosed, Is.False);
        }

        [Test]
        public void The_popup_stack_is_exhausted_before_any_panel_can_be_closed()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            GameplayPanel panel = null;
            uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);
            uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(uiService.TryClosePopup(), Is.True);
            Assert.That(uiService.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void Closing_every_popup_at_once_empties_the_popup_stack_and_leaves_the_panel_stack_untouched()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            GameplayPanel panel = null;
            uiService.ShowPanel<GameplayPanel>(shownPanel => panel = shownPanel);
            uiService.ShowPopup<ConfirmQuitPopup>();
            uiService.ShowPopup<ConfirmQuitPopup>();

            uiService.CloseAllPopups();

            Assert.That(uiService.HasPopup, Is.False);
            Assert.That(uiService.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(uiService.TryClosePanel(), Is.True);
        }

        [Test]
        public void Asking_whether_a_popup_is_up_agrees_with_the_popup_stack()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            uiService.ShowPanel<GameplayPanel>();

            Assert.That(uiService.HasPopup, Is.False);

            uiService.ShowPopup<ConfirmQuitPopup>();
            Assert.That(uiService.HasPopup, Is.True);

            uiService.ShowPopup<ConfirmQuitPopup>();
            Assert.That(uiService.HasPopup, Is.True);

            _ = uiService.TryClosePopup();
            Assert.That(uiService.HasPopup, Is.True);

            _ = uiService.TryClosePopup();
            Assert.That(uiService.HasPopup, Is.False);
        }

        [Test]
        public void Showing_a_panel_while_a_popup_is_up_throws()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            uiService.ShowPopup<ConfirmQuitPopup>();

            Assert.That(() => uiService.ShowPanel<GameplayPanel>(), Throws.InstanceOf<InvalidOperationException>());
        }
    }
}
