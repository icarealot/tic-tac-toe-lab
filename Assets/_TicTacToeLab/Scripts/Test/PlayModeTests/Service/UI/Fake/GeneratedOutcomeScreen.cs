#if UNITY_EDITOR
using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedOutcomeScreen : MonoBehaviour, IOutcomeScreen
    {
        private Action _onContinue;

        public void Setup(Outcome outcome, GameSetup setup, Action onContinue)
        {
            _onContinue = onContinue;
        }

        public void Acknowledge()
        {
            _onContinue?.Invoke();
        }
    }
}
#endif
