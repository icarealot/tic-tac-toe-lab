using System;
using TMPro;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class GameplayPanel : Panel, IGameplayPanel
    {
        [SerializeField] private TMP_Text _turnText;

        private Action _onBack;
        private IBoardSession _boardSession;

        public void Setup(IBoardSession boardSession, Action onBack)
        {
            _onBack = onBack;
            _boardSession = boardSession;
            _boardSession.TurnChanged += OnTurnChanged;
            OnTurnChanged(_boardSession.Turn);
        }

        public void Back()
        {
            _onBack?.Invoke();
        }

        private void OnDestroy()
        {
            if (_boardSession != null)
            {
                _boardSession.TurnChanged -= OnTurnChanged;
            }
        }

        private void OnTurnChanged(Mark turn)
        {
            _turnText.SetText($"{turn}'s turn");
        }
    }
}
