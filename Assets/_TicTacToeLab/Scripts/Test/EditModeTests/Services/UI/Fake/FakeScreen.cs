namespace TicTacToeLab.EditModeTests
{
    public abstract class FakeScreen
    {
        public bool IsVisible { get; private set; }
        public int PresentationCount { get; private set; }

        internal void Present()
        {
            PresentationCount++;
            IsVisible = true;
        }

        internal void Dismiss()
        {
            IsVisible = false;
        }
    }
}
