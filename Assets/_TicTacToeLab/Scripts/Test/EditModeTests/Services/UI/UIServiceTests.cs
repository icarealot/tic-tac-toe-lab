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
            UIService sut = new(fakeFactory, new StubUIRoot());

            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel firstPanel = fakeFactory.Panels[0];

            sut.ShowPanel<IGameplayPanel>();
            FakeGameplayPanel secondPanel = fakeFactory.Panels[1];

            // Assert
            Assert.That(firstPanel.IsVisible, Is.False);
            Assert.That(secondPanel.IsVisible, Is.True);

            // Act
            bool panelClosed = sut.TryClosePanel();

            // Assert
            Assert.That(panelClosed, Is.True);
            Assert.That(fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPanel }));
            Assert.That(firstPanel.IsVisible, Is.True);
        }

        [Test]
        public void Popups_stack_above_a_visible_panel_and_closing_the_top_reveals_the_one_beneath()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot());

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

            // Act
            bool popupClosed = sut.TryClosePopup();

            // Assert
            Assert.That(popupClosed, Is.True);
            Assert.That(fakeFactory.ReturnedWindows, Is.EqualTo(new IWindow[] { secondPopup }));
            Assert.That(firstPopup.IsVisible, Is.True);
            Assert.That(panel.IsVisible, Is.True);
        }

        [Test]
        public void The_callers_configuration_runs_while_the_window_is_hidden_and_before_it_becomes_visible()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot());

            // Act
            sut.ShowPopup<FakeConfirmQuitPopup>(popup => popup.Events.Add("configured"));

            // Assert
            Assert.That(fakeFactory.Popups[0].Events, Is.EqualTo(new[] { "hidden", "configured", "shown" }));
        }

        [Test]
        public void Closing_every_popup_empties_the_popup_stack_and_leaves_the_panel_stack_untouched()
        {
            // Arrange
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot());

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
            UIService sut = new(fakeFactory, new StubUIRoot());

            sut.ShowPopup<IConfirmQuitPopup>();

            Assert.That(() => sut.ShowPanel<IGameplayPanel>(), Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void Closing_from_an_empty_stack_reports_that_nothing_was_closed()
        {
            FakeFactoryService fakeFactory = new();
            UIService sut = new(fakeFactory, new StubUIRoot());

            Assert.That(sut.TryClosePanel(), Is.False);
            Assert.That(sut.TryClosePopup(), Is.False);
        }
    }
}
