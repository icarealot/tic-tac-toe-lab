using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public class GameplayPanel : Panel, IGameplayPanel
    {
        public Button BackButton => _backButton;

        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _turnText;

        private Action _onBack;
        private IBoardSession _boardSession;

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
        }

        public void Setup(IBoardSession boardSession, Action onBack)
        {
            _onBack = onBack;
            _boardSession = boardSession;
            _boardSession.TurnChanged += OnTurnChanged;
            OnTurnChanged(_boardSession.Turn);
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }

            if (_boardSession != null)
            {
                _boardSession.TurnChanged -= OnTurnChanged;
            }
        }

        private void OnBackClicked()
        {
            _onBack?.Invoke();
        }

        private void OnTurnChanged(Mark turn)
        {
            _turnText.SetText($"{turn}'s turn");
        }
    }
}
