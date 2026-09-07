namespace TicTacToeLab.Runtime
{
    public interface IWindow
    {
        public bool IsVisible { get; }
        public void Show();
        public void Hide();
    }
}
