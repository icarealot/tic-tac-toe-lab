using System;

namespace TicTacToeLab.Runtime
{
    public class ConfirmQuitPopup : Popup, IConfirmQuitPopup
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
