using System;

namespace TicTacToeLab.Runtime
{
    public interface IConfirmQuitPopup : IPopup
    {
        public void Setup(Action onYes, Action onNo);
    }
}
