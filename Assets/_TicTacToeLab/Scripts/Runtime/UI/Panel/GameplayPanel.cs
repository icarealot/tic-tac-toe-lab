using TMPro;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class GameplayPanel : Panel
    {
        [SerializeField] private TMP_Text _turnText;

        private BoardSession _boardSession;

        public void Setup(BoardSession boardSession)
        {
            _boardSession = boardSession;
            _boardSession.TurnChanged += OnTurnChanged;
            OnTurnChanged(_boardSession.Turn);
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
