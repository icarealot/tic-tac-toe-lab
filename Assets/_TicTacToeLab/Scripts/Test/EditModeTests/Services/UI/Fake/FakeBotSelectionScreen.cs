using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeBotSelectionScreen : FakeScreen, IBotSelectionScreen
    {
        public Action OnAmateur { get; private set; }
        public Action OnProfessional { get; private set; }
        public Action OnBack { get; private set; }

        public void Setup(Action onAmateur, Action onProfessional, Action onBack)
        {
            OnAmateur = onAmateur;
            OnProfessional = onProfessional;
            OnBack = onBack;
        }

        public void ClickAmateur()
        {
            OnAmateur?.Invoke();
        }

        public void ClickProfessional()
        {
            OnProfessional?.Invoke();
        }

        public void ClickBack()
        {
            OnBack?.Invoke();
        }
    }
}
