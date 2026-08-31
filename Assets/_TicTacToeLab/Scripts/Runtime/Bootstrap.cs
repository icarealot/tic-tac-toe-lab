using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private FactoryService _factoryServicePrefab;

        public void Awake()
        {
            Application.targetFrameRate = 60;

#if UNITY_EDITOR || ENABLE_LOGGING
            ILogService logService = new LogService();
#else
            ILogService logService = new NullLogService();
#endif

            FactoryService factoryService = Instantiate(_factoryServicePrefab);
            _ = factoryService.Get<Camera>();

            BoardView boardView = factoryService.Get<BoardView>();
            boardView.Construct(factoryService);

            logService.Log("Setup is done!");
        }
    }
}
