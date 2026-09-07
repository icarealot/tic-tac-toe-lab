namespace TicTacToeLab.Runtime
{
    public interface IUIFactoryService
    {
        public TPanel GetPanel<TPanel>() where TPanel : class, IPanel;
        public TPopup GetPopup<TPopup>() where TPopup : class, IPopup;
        public void Return(IWindow window);
    }
}
