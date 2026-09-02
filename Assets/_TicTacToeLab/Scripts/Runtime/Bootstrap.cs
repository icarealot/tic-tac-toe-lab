using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private FactoryService _factoryServicePrefab;

        private InputService _inputService;
        private BoardSession _boardSession;

        public void Awake()
        {
            Application.targetFrameRate = 60;

#if UNITY_EDITOR || ENABLE_LOGGING
            ILogService logService = new LogService();
#else
            ILogService logService = new NullLogService();
#endif

            FactoryService factoryService = Instantiate(_factoryServicePrefab);
            Camera mainCamera = factoryService.Get<Camera>();
            ICameraService cameraService = new CameraService(mainCamera);

            InputSystem_Actions inputActions = new();
            _inputService = new InputService(inputActions);

            BoardModel boardModel = new();
            BoardView boardView = factoryService.Get<BoardView>();
            BoardPresenter boardPresenter = new(boardModel, boardView, factoryService, _inputService, cameraService, logService);
            _boardSession = new BoardSession(boardPresenter);

            logService.Log("Setup is done!");
        }

        public void OnDestroy()
        {
            _boardSession?.Dispose();
            _inputService?.Dispose();
        }
    }
}
