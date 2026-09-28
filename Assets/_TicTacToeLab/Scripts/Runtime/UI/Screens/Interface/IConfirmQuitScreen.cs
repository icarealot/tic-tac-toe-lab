using System;

namespace TicTacToeLab.Runtime
{
    public interface IConfirmQuitScreen : IScreen
    {
        public void Setup(Action onConfirm, Action onCancel);
    }
}
