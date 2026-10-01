using System;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public sealed class HomeScreen : MonoBehaviour, IHomeScreen
    {
        [SerializeField] private Button _pvpButton;
        [SerializeField] private Button _pveButton;

        private Action _onPvp;
        private Action _onPve;

        private void Awake()
        {
            _pvpButton.onClick.AddListener(OnPvpClicked);
            _pveButton.onClick.AddListener(OnPveClicked);
        }

        public void Setup(Action onPvp, Action onPve)
        {
            _onPvp = onPvp;
            _onPve = onPve;
        }

        private void OnDestroy()
        {
            if (_pvpButton != null)
            {
                _pvpButton.onClick.RemoveListener(OnPvpClicked);
            }

            if (_pveButton != null)
            {
                _pveButton.onClick.RemoveListener(OnPveClicked);
            }
        }

        private void OnPvpClicked()
        {
            _onPvp?.Invoke();
        }

        private void OnPveClicked()
        {
            _onPve?.Invoke();
        }
    }
}
