using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeConfirmQuitScreen : FakeScreen, IConfirmQuitScreen
    {
        public Action OnConfirm { get; private set; }
        public Action OnCancel { get; private set; }

        public void Setup(Action onConfirm, Action onCancel)
        {
            OnConfirm = onConfirm;
            OnCancel = onCancel;
        }

        public void ClickConfirm()
        {
            OnConfirm?.Invoke();
        }

        public void ClickCancel()
        {
            OnCancel?.Invoke();
        }
    }
}
