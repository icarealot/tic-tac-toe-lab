using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public abstract class FakeWindow : IWindow
    {
        public bool IsVisible { get; private set; }

        /// <summary>Required by IWindow; UIService calls it on every show. The fake records nothing.</summary>
        public void Construct(ICoroutineService coroutineService)
        {
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }
    }
}
