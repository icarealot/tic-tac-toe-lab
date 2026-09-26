using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public sealed class OutcomePopup : MonoBehaviour
    {
        private const string WIN_TITLE_FORMAT = "{0} Wins!";
        private const string DRAW_TITLE = "Draw!";

        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _titleText;

        private Action _onContinue;

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
        }

        public void Setup(Outcome outcome, Action onContinue)
        {
            _titleText.SetText(TitleFor(outcome));
            _onContinue = onContinue;
        }

        private void OnDestroy()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueClicked);
            }
        }

        private void OnContinueClicked()
        {
            _onContinue?.Invoke();
        }

        private static string TitleFor(Outcome outcome)
        {
            return outcome switch
            {
                Outcome.XWin => string.Format(WIN_TITLE_FORMAT, Mark.X),
                Outcome.OWin => string.Format(WIN_TITLE_FORMAT, Mark.O),
                Outcome.Draw => DRAW_TITLE,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The outcome popup can only present a terminal outcome.")
            };
        }
    }
}
