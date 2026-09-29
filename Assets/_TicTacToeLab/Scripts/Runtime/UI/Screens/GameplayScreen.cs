using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public sealed class GameplayScreen : MonoBehaviour, IGameplayScreen
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _turnText;

        private Action _onBack;
        private BoardPresenter _boardPresenter;
        private GameSetup _setup;

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackClicked);
        }

        public void Setup(BoardPresenter boardPresenter, GameSetup setup, Action onBack)
        {
            _onBack = onBack;
            _setup = setup;
            _boardPresenter = boardPresenter;
            _boardPresenter.TurnChanged += OnTurnChanged;
            OnTurnChanged(_boardPresenter.Turn);
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }

            if (_boardPresenter != null)
            {
                _boardPresenter.TurnChanged -= OnTurnChanged;
                _boardPresenter = null;
            }
        }

        private void OnBackClicked()
        {
            _onBack?.Invoke();
        }

        private void OnTurnChanged(Mark turn)
        {
            _turnText.SetText(TurnTextFor(turn));
        }

        private string TurnTextFor(Mark turn)
        {
            if (_setup.Mode == GameMode.Pve)
            {
                return turn == Mark.X ? "Your turn (X)" : "Bot is thinking… (O)";
            }

            return $"{turn}'s turn";
        }
    }
}
