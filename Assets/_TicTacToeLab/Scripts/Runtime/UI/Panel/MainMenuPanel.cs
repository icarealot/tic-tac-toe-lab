using System;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public class MainMenuPanel : Panel, IMainMenuPanel
    {
        public Button StartButton => _startButton;

        [SerializeField] private Button _startButton;

        private Action _onStart;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnStartClicked);
        }

        public void Setup(Action onStart)
        {
            _onStart = onStart;
        }

        private void OnDestroy()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(OnStartClicked);
            }
        }

        private void OnStartClicked()
        {
            _onStart?.Invoke();
        }
    }
}
