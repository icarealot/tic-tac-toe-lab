using System;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public class ConfirmQuitPopup : MonoBehaviour
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        private Action _onYes;
        private Action _onNo;

        private void Awake()
        {
            _yesButton.onClick.AddListener(OnYesClicked);
            _noButton.onClick.AddListener(OnNoClicked);
        }

        public void Setup(Action onYes, Action onNo)
        {
            _onYes = onYes;
            _onNo = onNo;
        }

        private void OnDestroy()
        {
            if (_yesButton != null)
            {
                _yesButton.onClick.RemoveListener(OnYesClicked);
            }

            if (_noButton != null)
            {
                _noButton.onClick.RemoveListener(OnNoClicked);
            }
        }

        private void OnYesClicked()
        {
            _onYes?.Invoke();
        }

        private void OnNoClicked()
        {
            _onNo?.Invoke();
        }
    }
}
