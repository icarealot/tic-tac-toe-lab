using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeGameplayPanel : FakeWindow, IGameplayPanel
    {
        public void Setup(IBoardSession boardSession, Action onBack)
        {
        }
    }
}
