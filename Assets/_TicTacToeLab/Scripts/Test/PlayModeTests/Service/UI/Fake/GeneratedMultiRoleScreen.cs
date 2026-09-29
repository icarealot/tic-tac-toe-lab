#if UNITY_EDITOR
using System;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class GeneratedMultiRoleScreen : MonoBehaviour, IHomeScreen, IGameplayScreen
    {
        public void Setup(Action onPvp, Action onPve)
        {
        }

        public void Setup(BoardPresenter boardPresenter, GameSetup setup, Action onBack)
        {
        }
    }
}
#endif
