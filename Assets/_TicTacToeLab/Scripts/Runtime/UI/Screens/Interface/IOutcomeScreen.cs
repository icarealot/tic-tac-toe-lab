using System;

namespace TicTacToeLab.Runtime
{
    public interface IOutcomeScreen : IScreen
    {
        public void Setup(Outcome outcome, Action onContinue);
    }
}
