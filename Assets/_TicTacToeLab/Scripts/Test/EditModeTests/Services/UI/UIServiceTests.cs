using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class UIServiceTests
    {
        private sealed class StubUIRoot : IUIRoot
        {
            public RectTransform PanelLayer => null;
            public RectTransform PopupLayer => null;
        }

        private FakeFactoryService _fakeFactory;
        private UIService _uiService;

        [SetUp]
        public void SetUp()
        {
            _fakeFactory = new FakeFactoryService();
            _uiService = new UIService(_fakeFactory, new StubUIRoot(), new FakeCoroutineService());
        }

        [Test]
        public void Showing_a_second_panel_hides_but_preserves_the_first_until_the_top_panel_closes()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel firstPanel = _fakeFactory.Panels[0];
            Assert.That(firstPanel.IsVisible, Is.True);

            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel secondPanel = _fakeFactory.Panels[1];

            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(_fakeFactory.ReturnedWindows, Is.Empty);

            Assert.That(_uiService.TryClosePanel(), Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
        }

        [Test]
        public void A_popup_leaves_the_panel_visible_while_popups_stack_and_close_above_it()
        {
            _uiService.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = _fakeFactory.Panels[0];

            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup firstPopup = _fakeFactory.Popups[0];
            Assert.That(firstPopup.IsVisible, Is.True);
            Assert.That(panel.IsVisible, Is.True);

            _uiService.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup secondPopup = _fakeFactory.Popups[1];

            Assert.That(firstPopup.IsVisible, Is.False);
            Assert.That(panel.IsVisible, Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Is.Empty);

            Assert.That(_uiService.TryClosePopup(), Is.True);
            Assert.That(_fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPopup }));
            Assert.That(firstPopup.IsVisible, Is.True);
            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Exhausting_the_popup_stack_leaves_the_panel_stack_intact()
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
        public void Public_popup_presence_tracks_whether_any_popup_is_up()
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

        [Test]
        public void Closing_from_an_empty_stack_reports_that_nothing_was_closed()
        {
            Assert.That(_uiService.TryClosePanel(), Is.False);
            Assert.That(_uiService.TryClosePopup(), Is.False);
        }
    }
}
