using System;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public sealed class HomeScreen : MonoBehaviour, IHomeScreen
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _pveButton;

        private Action _onPvp;
        private Action _onPve;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnPvpClicked);

            if (_pveButton != null)
            {
                _pveButton.onClick.AddListener(OnPveClicked);
            }
        }

        public void Setup(Action onPvp, Action onPve)
        {
            _onPvp = onPvp;
            _onPve = onPve;
        }

        private void OnDestroy()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(OnPvpClicked);
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
