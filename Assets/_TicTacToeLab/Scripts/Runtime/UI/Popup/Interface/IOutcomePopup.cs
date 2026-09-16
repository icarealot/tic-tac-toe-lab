using System;

namespace TicTacToeLab.Runtime
{
    public interface IOutcomePopup : IPopup
    {
        public void Setup(Outcome outcome, Mark turn, Action onContinue);
    }
}
