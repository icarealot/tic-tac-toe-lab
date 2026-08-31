using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private FactoryService _factoryServicePrefab;

        private InputService _inputService;
        private BoardPresenter _boardPresenter;

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
            _boardPresenter = new BoardPresenter(boardModel, boardView, factoryService, _inputService, cameraService, logService);

            logService.Log("Setup is done!");
        }

        public void OnDestroy()
        {
            _boardPresenter?.Dispose();
            _inputService?.Dispose();
        }
    }
}
