#if UNITY_EDITOR
using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedGameplayScreen : MonoBehaviour, IGameplayScreen
    {
        private Action _onBack;

        public void Setup(BoardPresenter boardPresenter, GameSetup setup, Action onBack)
        {
            _onBack = onBack;
        }

        public void PressBack()
        {
            _onBack?.Invoke();
        }
    }
}
#endif
