#if UNITY_EDITOR
using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedHomeScreen : MonoBehaviour, IHomeScreen
    {
        private Action _onPvp;

        public void Setup(Action onPvp, Action onPve)
        {
            _onPvp = onPvp;
        }

        public void SelectPvp()
        {
            _onPvp?.Invoke();
        }
    }
}
#endif
