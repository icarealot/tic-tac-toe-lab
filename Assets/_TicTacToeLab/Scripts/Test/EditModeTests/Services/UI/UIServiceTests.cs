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

        [Test]
        public void Showing_a_second_panel_hides_but_preserves_the_first_until_the_top_panel_closes()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot(), new FakeCoroutineService());

            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel firstPanel = fakeFactory.Panels[0];

            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel secondPanel = fakeFactory.Panels[1];

            // Act
            bool panelClosed = sut.TryClosePanel();

            // Assert
            Assert.That(panelClosed, Is.True);
            Assert.That(fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
        }

        [Test]
        public void A_popup_leaves_the_panel_visible_while_popups_stack_and_close_above_it()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot(), new FakeCoroutineService());

            // Act
            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = fakeFactory.Panels[0];

            sut.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup firstPopup = fakeFactory.Popups[0];

            sut.ShowPopup<IConfirmQuitPopup>();
            FakeConfirmQuitPopup secondPopup = fakeFactory.Popups[1];

            // Assert
            Assert.That(secondPopup.IsVisible, Is.True);
            Assert.That(firstPopup.IsVisible, Is.False);
            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Exhausting_the_popup_stack_leaves_the_panel_stack_intact()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot(), new FakeCoroutineService());
            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = fakeFactory.Panels[0];
            sut.ShowPopup<IConfirmQuitPopup>();

            // Act
            _ = sut.TryClosePopup();
            _ = sut.TryClosePopup();

            // Assert
            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Closing_every_popup_empties_the_popup_stack_and_leaves_the_panel_stack_untouched()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot(), new FakeCoroutineService());

            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel panel = fakeFactory.Panels[0];
            sut.ShowPopup<IConfirmQuitPopup>();
            sut.ShowPopup<IConfirmQuitPopup>();

            // Act
            sut.CloseAllPopups();

            // Assert
            Assert.That(sut.HasPopup, Is.False);
            Assert.That(sut.TryClosePopup(), Is.False);
            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void Showing_a_panel_while_a_popup_is_up_throws()
        {
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot(), new FakeCoroutineService());

            sut.ShowPopup<IConfirmQuitPopup>();

            Assert.That(() => sut.ShowPanel<IGameplayPanel>(), Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void Closing_from_an_empty_stack_reports_that_nothing_was_closed()
        {
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot(), new FakeCoroutineService());

            Assert.That(sut.TryClosePanel(), Is.False);
            Assert.That(sut.TryClosePopup(), Is.False);
        }
    }
}
