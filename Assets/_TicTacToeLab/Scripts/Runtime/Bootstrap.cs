using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private FactoryService _factoryServicePrefab;

        public void Awake()
        {
            Application.targetFrameRate = 60;

            FactoryService factoryService = Instantiate(_factoryServicePrefab);
            _ = factoryService.Get<Camera>();
        }
    }
}
