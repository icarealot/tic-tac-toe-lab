using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private FactoryService _factoryServicePrefab;

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

            FactoryService factoryService = Instantiate(_factoryServicePrefab);

            IMainCamera mainCamera = factoryService.Get<IMainCamera>();
            ICameraService cameraService = new CameraService(mainCamera.Camera);
            ICoroutineService coroutineService = factoryService.Get<ICoroutineService>();

            InputSystem_Actions inputActions = new();
            _inputService = new InputService(inputActions);

            BoardModel boardModel = new();
            IBoardView boardView = factoryService.Get<IBoardView>();
            boardView.Construct(factoryService, boardModel.Dimension, boardModel.GetCellPlacements());
            BoardPresenter boardPresenter = new(boardModel, boardView, _inputService, cameraService, logService);
            _boardSession = new BoardSession(boardPresenter);

            IUIRoot uiRoot = factoryService.Get<IUIRoot>();
            IUIService uiService = new UIService(factoryService, uiRoot, coroutineService);

            _stateMachine = new AppStateMachine(_inputService);
            MainMenuState mainMenuState = new(_boardSession, _stateMachine, uiService);
            GameplayState gameplayState = new(_boardSession, _stateMachine, uiService, _inputService);
            GameCompleteState gameCompleteState = new(_boardSession, _stateMachine, uiService, coroutineService, _inputService);
            _stateMachine.Add(mainMenuState);
            _stateMachine.Add(gameplayState);
            _stateMachine.Add(gameCompleteState);
            _stateMachine.ChangeState<MainMenuState>();

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
