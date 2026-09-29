using System;

namespace TicTacToeLab.Runtime
{
    public interface IBotSelectionScreen : IScreen
    {
        public void Setup(Action onAmateur, Action onProfessional, Action onBack);
    }
}
