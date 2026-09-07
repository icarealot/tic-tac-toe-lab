using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private ComponentFactoryService _factoryServicePrefab;

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

            ComponentFactoryService factoryService = Instantiate(_factoryServicePrefab);
            Camera mainCamera = factoryService.Get<Camera>();
            ICameraService cameraService = new CameraService(mainCamera);
            ICoroutineService coroutineService = factoryService.Get<CoroutineService>();

            InputSystem_Actions inputActions = new();
            _inputService = new InputService(inputActions);

            BoardModel boardModel = new();
            BoardView boardView = factoryService.Get<BoardView>();
            boardView.Construct(factoryService, boardModel.Dimension, boardModel.GetCellPlacements());
            BoardPresenter boardPresenter = new(boardModel, boardView, _inputService, cameraService, logService);
            _boardSession = new BoardSession(boardPresenter);

            UIRoot uiRoot = factoryService.Get<UIRoot>();
            IUIFactoryService uiFactoryService = new UIFactoryService(factoryService, uiRoot, coroutineService);
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
