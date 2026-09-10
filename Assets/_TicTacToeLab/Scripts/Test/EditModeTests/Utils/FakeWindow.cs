using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public abstract class FakeWindow : IWindow
    {
        public bool IsVisible { get; private set; }
        public List<ICoroutineService> ConstructionServices { get; } = new();

        public void Construct(ICoroutineService coroutineService)
        {
            ConstructionServices.Add(coroutineService);
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
