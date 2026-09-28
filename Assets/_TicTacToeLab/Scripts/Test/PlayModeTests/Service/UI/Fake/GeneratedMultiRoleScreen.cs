#if UNITY_EDITOR
using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedMultiRoleScreen : MonoBehaviour, IHomeScreen, IGameplayScreen
    {
        public void Setup(Action onStart)
        {
        }

        public void Setup(BoardPresenter boardPresenter, Action onBack)
        {
        }
    }
}
#endif
