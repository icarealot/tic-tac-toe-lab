using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public class OutcomePopup : Popup, IOutcomePopup
    {
        private const string WIN_TITLE_FORMAT = "{0} Wins!";
        private const string DRAW_TITLE = "Draw!";

        public Button ContinueButton => _continueButton;

        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _titleText;

        private Action _onContinue;

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
        }

        public void Setup(Outcome outcome, Mark turn, Action onContinue)
        {
            _titleText.SetText(TitleFor(outcome, turn));
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

        private static string TitleFor(Outcome outcome, Mark turn)
        {
            return outcome switch
            {
                Outcome.Win => string.Format(WIN_TITLE_FORMAT, turn),
                Outcome.Draw => DRAW_TITLE,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The outcome popup can only present a terminal outcome.")
            };
        }
    }
}
