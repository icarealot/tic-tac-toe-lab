using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeOutcomePopup : FakeWindow, IOutcomePopup
    {
        public Outcome Outcome { get; private set; }
        public Mark Turn { get; private set; }

        private Action _onContinue;

        public void Setup(Outcome outcome, Mark turn, Action onContinue)
        {
            Outcome = outcome;
            Turn = turn;
            _onContinue = onContinue;
        }

        public void Continue()
        {
            _onContinue?.Invoke();
        }
    }
}
