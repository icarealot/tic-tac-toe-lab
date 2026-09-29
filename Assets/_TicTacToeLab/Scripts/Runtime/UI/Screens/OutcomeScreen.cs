using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToeLab.Runtime
{
    public sealed class OutcomeScreen : MonoBehaviour, IOutcomeScreen
    {
        private const string PVP_WIN_TITLE_FORMAT = "{0} Wins!";
        private const string PVE_X_WIN_TITLE = "You Win!";
        private const string PVE_O_WIN_TITLE = "Bot Wins!";
        private const string DRAW_TITLE = "Draw!";

        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _titleText;

        private Action _onContinue;

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
        }

        public void Setup(Outcome outcome, GameSetup setup, Action onContinue)
        {
            _titleText.SetText(TitleFor(outcome, setup));
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

        private static string TitleFor(Outcome outcome, GameSetup setup)
        {
            if (setup.Mode == GameMode.Pve)
            {
                return outcome switch
                {
                    Outcome.XWin => PVE_X_WIN_TITLE,
                    Outcome.OWin => PVE_O_WIN_TITLE,
                    Outcome.Draw => DRAW_TITLE,
                    _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The outcome screen can only present a terminal outcome.")
                };
            }

            return outcome switch
            {
                Outcome.XWin => string.Format(PVP_WIN_TITLE_FORMAT, Mark.X),
                Outcome.OWin => string.Format(PVP_WIN_TITLE_FORMAT, Mark.O),
                Outcome.Draw => DRAW_TITLE,
                _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The outcome screen can only present a terminal outcome.")
            };
        }
    }
}
