using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeOutcomeScreen : FakeScreen, IOutcomeScreen
    {
        public Outcome ShownOutcome { get; private set; }
        public GameSetup ShownSetup { get; private set; }
        public Action OnContinue { get; private set; }

        public void Setup(Outcome outcome, GameSetup setup, Action onContinue)
        {
            ShownOutcome = outcome;
            ShownSetup = setup;
            OnContinue = onContinue;
        }

        public void ClickContinue()
        {
            OnContinue?.Invoke();
        }
    }
}
