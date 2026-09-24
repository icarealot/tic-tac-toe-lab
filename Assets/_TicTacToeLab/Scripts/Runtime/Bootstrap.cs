using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private Camera _cameraPrefab;
        [SerializeField] private BoardView _boardViewPrefab;
        [SerializeField] private ApplicationUI _applicationUIPrefab;
        [SerializeField] private InputService _inputServicePrefab;
        [SerializeField] private DelayScheduler _delaySchedulerPrefab;

        private Camera _camera;
        private BoardView _boardView;
        private ApplicationUI _applicationUI;
        private InputService _inputService;
        private DelayScheduler _delayScheduler;
        private BoardPresenter _boardPresenter;
        private ApplicationFlow _applicationFlow;

        public void Awake()
        {
            if (!ValidateAdapterPrefabs())
            {
                return;
            }

            Application.targetFrameRate = 60;

            _camera = InstantiateRoot(_cameraPrefab);
            _boardView = InstantiateRoot(_boardViewPrefab);
            _applicationUI = InstantiateRoot(_applicationUIPrefab);
            _inputService = InstantiateRoot(_inputServicePrefab);
            _delayScheduler = InstantiateRoot(_delaySchedulerPrefab);

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
            DisposeCollaborators();
            DestroyOwnedRoots();
        }

        private bool ValidateAdapterPrefabs()
        {
            bool hasCameraPrefab = IsPrefabAssigned(_cameraPrefab, "MainCamera");
            bool hasBoardViewPrefab = IsPrefabAssigned(_boardViewPrefab, "BoardView");
            bool hasApplicationUIPrefab = IsPrefabAssigned(_applicationUIPrefab, "ApplicationUI");
            bool hasInputServicePrefab = IsPrefabAssigned(_inputServicePrefab, "InputService");
            bool hasDelaySchedulerPrefab = IsPrefabAssigned(_delaySchedulerPrefab, "DelayScheduler");

            return hasCameraPrefab
                && hasBoardViewPrefab
                && hasApplicationUIPrefab
                && hasInputServicePrefab
                && hasDelaySchedulerPrefab;
        }

        private static bool IsPrefabAssigned(Component adapterPrefab, string adapterRole)
        {
            if (adapterPrefab != null)
            {
                return true;
            }

            Debug.LogError($"Bootstrap cannot start because the {adapterRole} adapter prefab is not assigned.");
            return false;
        }

        private static T InstantiateRoot<T>(T adapterPrefab) where T : Component
        {
            T adapter = Instantiate(adapterPrefab);
            adapter.name = adapterPrefab.name;
            return adapter;
        }

        private void DisposeCollaborators()
        {
            if (_applicationFlow != null)
            {
                _applicationFlow.Dispose();
                _applicationFlow = null;
            }

            if (_boardPresenter != null)
            {
                _boardPresenter.Dispose();
                _boardPresenter = null;
            }

            if (_inputService != null)
            {
                _inputService.Dispose();
            }
        }

        private void DestroyOwnedRoots()
        {
            DestroyOwnedRoot(_camera);
            DestroyOwnedRoot(_boardView);
            DestroyOwnedRoot(_applicationUI);
            DestroyOwnedRoot(_inputService);
            DestroyOwnedRoot(_delayScheduler);

            _camera = null;
            _boardView = null;
            _applicationUI = null;
            _inputService = null;
            _delayScheduler = null;
        }

        private static void DestroyOwnedRoot(Component ownedRoot)
        {
            if (ownedRoot == null)
            {
                return;
            }

            Destroy(ownedRoot.gameObject);
        }
    }
}
