using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ApplicationUI _applicationUI;
        [SerializeField] private InputService _inputService;
        [SerializeField] private DelayScheduler _delayScheduler;

        private BoardPresenter _boardPresenter;
        private ApplicationFlow _applicationFlow;

        public void Awake()
        {
            Application.targetFrameRate = 60;

            BoardModel boardModel = new();
            BoardLayout boardLayout = new(boardModel.Dimension);
            ICameraService cameraService = new CameraService(_camera);

            _boardView.Construct(boardModel.Dimension, boardLayout.GetCellPlacements());
            _boardPresenter = new BoardPresenter(boardModel, boardLayout, _boardView, _inputService, cameraService);
            _applicationFlow = new ApplicationFlow(_boardPresenter, _applicationUI, _delayScheduler, _inputService);
            _applicationFlow.Start();
        }

        public void OnDestroy()
        {
            _applicationFlow?.Dispose();
            _applicationFlow = null;

            _boardPresenter?.Dispose();
            _boardPresenter = null;

            if (_inputService != null)
            {
                _inputService.Dispose();
            }
        }
    }
}
