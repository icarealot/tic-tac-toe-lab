using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class UIFactoryService : IUIFactoryService
    {
        private readonly IComponentFactoryService _componentFactoryService;
        private readonly IUIRoot _uiRoot;
        private readonly ICoroutineService _coroutineService;

        public UIFactoryService(
            IComponentFactoryService componentFactoryService,
            IUIRoot uiRoot,
            ICoroutineService coroutineService)
        {
            _componentFactoryService = componentFactoryService;
            _uiRoot = uiRoot;
            _coroutineService = coroutineService;
        }

        public TPanel GetPanel<TPanel>() where TPanel : class, IPanel
        {
            return Get<TPanel>(_uiRoot.PanelLayer);
        }

        public TPopup GetPopup<TPopup>() where TPopup : class, IPopup
        {
            return Get<TPopup>(_uiRoot.PopupLayer);
        }

        public void Return(IWindow window)
        {
            _componentFactoryService.Return((Component)(object)window);
        }

        private TWindow Get<TWindow>(Transform parent) where TWindow : class, IWindow
        {
            TWindow window = (TWindow)(object)_componentFactoryService.Get(typeof(TWindow), parent);
            ((Window)(object)window).Construct(_coroutineService);
            return window;
        }
    }
}
