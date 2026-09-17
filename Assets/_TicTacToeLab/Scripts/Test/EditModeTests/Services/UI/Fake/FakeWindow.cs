using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public abstract class FakeWindow : IWindow
    {
        public bool IsVisible { get; private set; }

        public List<string> Events { get; } = new();

        public void Show()
        {
            IsVisible = true;
            Events.Add("shown");
        }

        public void Hide()
        {
            IsVisible = false;
            Events.Add("hidden");
        }
    }
}
