using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeConfirmQuitPopup : FakeWindow, IConfirmQuitPopup
    {
        private Action _onYes;
        private Action _onNo;

        public void Setup(Action onYes, Action onNo)
        {
            _onYes = onYes;
            _onNo = onNo;
        }

        public void Yes()
        {
            _onYes?.Invoke();
        }

        public void No()
        {
            _onNo?.Invoke();
        }
    }
}
