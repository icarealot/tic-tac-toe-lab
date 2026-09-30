using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private Camera _cameraPrefab;
        [SerializeField] private BoardView _boardViewPrefab;
        [SerializeField] private AppUI _appUIPrefab;
        [SerializeField] private DelayScheduler _delaySchedulerPrefab;

        private Camera _camera;
        private BoardView _boardView;
        private AppUI _appUI;
        private InputService _inputService;
        private DelayScheduler _delayScheduler;
        private BoardPresenter _boardPresenter;
        private AppStateMachine _appStateMachine;

        public void Awake()
        {
            if (!ValidateAdapterPrefabs())
            {
                return;
            }

            Application.targetFrameRate = 60;

            _camera = InstantiateRoot(_cameraPrefab);
            _boardView = InstantiateRoot(_boardViewPrefab);
            _appUI = InstantiateRoot(_appUIPrefab);
            _delayScheduler = InstantiateRoot(_delaySchedulerPrefab);
            _inputService = new InputService();

            BoardModel boardModel = new();
            BoardLayout boardLayout = new(boardModel.Dimension);
            ICameraService cameraService = new CameraService(_camera);

            _boardView.Construct(boardModel.Dimension, boardLayout.GetCellPlacements());
            _boardPresenter = new BoardPresenter(boardModel, boardLayout, _boardView, _inputService, cameraService);
            IRandomService randomService = new RandomService();
            _appStateMachine = new AppStateMachine(
                _boardPresenter,
                _appUI,
                _delayScheduler,
                _inputService,
                randomService);
            _appStateMachine.Start();
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
            bool hasAppUIPrefab = IsPrefabAssigned(_appUIPrefab, "AppUI");
            bool hasDelaySchedulerPrefab = IsPrefabAssigned(_delaySchedulerPrefab, "DelayScheduler");

            return hasCameraPrefab
                && hasBoardViewPrefab
                && hasAppUIPrefab
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
            if (_appStateMachine != null)
            {
                _appStateMachine.Dispose();
                _appStateMachine = null;
            }

            if (_boardPresenter != null)
            {
                _boardPresenter.Dispose();
                _boardPresenter = null;
            }

            if (_inputService != null)
            {
                _inputService.Dispose();
                _inputService = null;
            }
        }

        private void DestroyOwnedRoots()
        {
            DestroyOwnedRoot(_camera);
            DestroyOwnedRoot(_boardView);
            DestroyOwnedRoot(_appUI);
            DestroyOwnedRoot(_delayScheduler);

            _camera = null;
            _boardView = null;
            _appUI = null;
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
