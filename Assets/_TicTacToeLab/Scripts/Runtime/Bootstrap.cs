using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private ComponentFactoryService _componentFactoryServicePrefab;

        private InputService _inputService;
        private BoardSession _boardSession;
        private AppStateMachine _stateMachine;

        public void Awake()
        {
            Application.targetFrameRate = 60;

#if UNITY_EDITOR || ENABLE_LOGGING
            ILogService logService = new LogService();
#else
            ILogService logService = new NullLogService();
#endif

            ComponentFactoryService componentFactoryService = Instantiate(_componentFactoryServicePrefab);
            IFactoryService factoryService = componentFactoryService.GetComponent<FactoryService>();

            IMainCamera mainCamera = factoryService.Get<IMainCamera>();
            ICameraService cameraService = new CameraService(mainCamera.Camera);
            ICoroutineService coroutineService = factoryService.Get<ICoroutineService>();

            InputSystem_Actions inputActions = new();
            _inputService = new InputService(inputActions);

            BoardModel boardModel = new();
            BoardView boardView = componentFactoryService.Get<BoardView>();
            boardView.Construct(componentFactoryService, boardModel.Dimension, boardModel.GetCellPlacements());
            BoardPresenter boardPresenter = new(boardModel, boardView, _inputService, cameraService, logService);
            _boardSession = new BoardSession(boardPresenter);

            IUIRoot uiRoot = factoryService.Get<IUIRoot>();
            IUIFactoryService uiFactoryService = new UIFactoryService(componentFactoryService, uiRoot, coroutineService);
            IUIService uiService = new UIService(uiFactoryService);

            _stateMachine = new AppStateMachine(_inputService);
            GameplayState gameplayState = new(_boardSession, _stateMachine, uiService, _inputService, logService);
            GameCompleteState gameCompleteState = new(_boardSession, _stateMachine, coroutineService);
            _stateMachine.Add(gameplayState);
            _stateMachine.Add(gameCompleteState);
            _stateMachine.ChangeState<GameplayState>();

            logService.Log("Setup is done!");
        }

        public void OnDestroy()
        {
            _stateMachine?.Dispose();
            _boardSession?.Dispose();
            _inputService?.Dispose();
        }
    }
}
