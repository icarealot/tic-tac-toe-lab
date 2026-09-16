using System;
using TMPro;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class OutcomePopup : Popup, IOutcomePopup
    {
        private const string WIN_TITLE_FORMAT = "{0} Wins!";
        private const string DRAW_TITLE = "Draw!";

        [SerializeField] private TMP_Text _titleText;

        private Action _onContinue;

        public void Setup(Outcome outcome, Mark turn, Action onContinue)
        {
            _titleText.SetText(TitleFor(outcome, turn));
            _onContinue = onContinue;
        }

        public void Continue()
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
