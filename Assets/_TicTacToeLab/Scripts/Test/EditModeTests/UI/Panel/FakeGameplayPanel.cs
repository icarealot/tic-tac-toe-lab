using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeGameplayPanel : FakeWindow, IGameplayPanel
    {
        /// <summary>Required by IGameplayPanel; panel configuration is not part of the stack behavior under test.</summary>
        public void Setup(IBoardSession boardSession, Action onBack)
        {
        }
    }
}
