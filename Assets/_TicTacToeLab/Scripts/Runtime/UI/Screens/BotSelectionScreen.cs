using System;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public sealed class BotSelectionScreen : MonoBehaviour, IBotSelectionScreen
    {
        [SerializeField] private Button _amateurButton;
        [SerializeField] private Button _professionalButton;
        [SerializeField] private Button _backButton;

        private Action _onAmateur;
        private Action _onProfessional;
        private Action _onBack;

        private void Awake()
        {
            _amateurButton.onClick.AddListener(OnAmateurClicked);
            _professionalButton.onClick.AddListener(OnProfessionalClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        public void Setup(Action onAmateur, Action onProfessional, Action onBack)
        {
            _onAmateur = onAmateur;
            _onProfessional = onProfessional;
            _onBack = onBack;
        }

        private void OnDestroy()
        {
            if (_amateurButton != null)
            {
                _amateurButton.onClick.RemoveListener(OnAmateurClicked);
            }

            if (_professionalButton != null)
            {
                _professionalButton.onClick.RemoveListener(OnProfessionalClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        private void OnAmateurClicked()
        {
            _onAmateur?.Invoke();
        }

        private void OnProfessionalClicked()
        {
            _onProfessional?.Invoke();
        }

        private void OnBackClicked()
        {
            _onBack?.Invoke();
        }
    }
}
