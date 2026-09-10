namespace TicTacToeLab.Runtime
{
    public interface IWindow
    {
        public bool IsVisible { get; }
        public void Construct(ICoroutineService coroutineService);
        public void Show();
        public void Hide();
    }
}
