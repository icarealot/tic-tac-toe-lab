using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public abstract class FakeWindow : IWindow
    {
        public bool IsVisible { get; private set; }

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
