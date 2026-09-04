using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class ConfirmQuitPopupTests
    {
        [Test]
        public void A_popup_reports_yes_and_no_through_the_callbacks_it_was_configured_with()
        {
            FakeFactoryService fakeFactoryService = new();
            ConfirmQuitPopup popup = fakeFactoryService.Get<ConfirmQuitPopup>();
            int yesCount = 0;
            int noCount = 0;
            popup.Setup(onYes: () => yesCount++, onNo: () => noCount++);

            popup.Yes();
            popup.No();

            Assert.That(yesCount, Is.EqualTo(1));
            Assert.That(noCount, Is.EqualTo(1));
        }

        [Test]
        public void A_popup_that_is_answered_neither_closes_itself_nor_navigates()
        {
            UIRoot uiRoot = TestUIRoot.Create();
            FakeFactoryService fakeFactoryService = new();
            UIService uiService = new(fakeFactoryService, uiRoot, new FakeCoroutineService());
            ConfirmQuitPopup shownPopup = null;
            uiService.ShowPopup<ConfirmQuitPopup>(popup => popup.Setup(onYes: () => { }, onNo: () => { }));
            shownPopup = uiRoot.PopupLayer.GetComponentInChildren<ConfirmQuitPopup>();

            shownPopup.Yes();
            shownPopup.No();

            // Answering only reports. Taking the popup off its stack is the asking state's decision.
            Assert.That(uiService.HasPopup, Is.True);
            Assert.That(shownPopup.IsVisible, Is.True);
            Assert.That(fakeFactoryService.ReturnedInstances, Is.Empty);
        }
    }
}
