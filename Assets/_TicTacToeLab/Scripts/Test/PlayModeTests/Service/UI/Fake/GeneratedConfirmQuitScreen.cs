#if UNITY_EDITOR
using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedConfirmQuitScreen : MonoBehaviour, IConfirmQuitScreen
    {
        private Action _onConfirm;

        public void Setup(Action onConfirm, Action onCancel)
        {
            _onConfirm = onConfirm;
        }

        public void Confirm()
        {
            _onConfirm?.Invoke();
        }
    }
}
#endif
