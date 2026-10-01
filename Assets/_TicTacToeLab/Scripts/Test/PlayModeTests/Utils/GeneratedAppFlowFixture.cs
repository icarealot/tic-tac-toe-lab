#if UNITY_EDITOR
using System;
using System.Collections;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Composes the application state machine with production AppUI and generated screen templates for flow checks.
    /// </summary>
    public sealed class GeneratedAppFlowFixture
    {
        public GeneratedAppUIFixture UI { get; }
        public BoardPresenter BoardPresenter { get; }
        public GeneratedFlowDelayScheduler DelayScheduler { get; }

        private readonly AppStateMachine _stateMachine;

        public GeneratedAppFlowFixture()
        {
            UI = new GeneratedAppUIFixture();
            GeneratedHomeScreen home = UI.CreateScreenTemplate<GeneratedHomeScreen>();
            GeneratedGameplayScreen gameplay = UI.CreateScreenTemplate<GeneratedGameplayScreen>();
            GeneratedConfirmQuitScreen confirmQuit = UI.CreateScreenTemplate<GeneratedConfirmQuitScreen>();
            GeneratedOutcomeScreen outcome = UI.CreateScreenTemplate<GeneratedOutcomeScreen>();

            AppUI appUI = UI.CreateInactiveAppUI(
                new ScreenRegistration(home, ScreenLayer.Base),
                new ScreenRegistration(gameplay, ScreenLayer.Base),
                new ScreenRegistration(confirmQuit, ScreenLayer.Popup),
                new ScreenRegistration(outcome, ScreenLayer.Popup));
            appUI.gameObject.SetActive(true);

            GeneratedFlowInputService inputService = new();
            BoardModel boardModel = new();
            BoardPresenter = new BoardPresenter(
                boardModel,
                new BoardLayout(boardModel.Dimension),
                new GeneratedFlowBoardView(),
                inputService,
                new GeneratedFlowCameraService());
            DelayScheduler = new GeneratedFlowDelayScheduler();
            _stateMachine = new AppStateMachine(
                BoardPresenter,
                appUI,
                DelayScheduler,
                inputService,
                new GeneratedFlowRandomService());
            _stateMachine.Start();
        }

        public IEnumerator IE_DestroyAll()
        {
            _stateMachine.Dispose();
            BoardPresenter.Dispose();
            yield return UI.IE_DestroyAll();
        }
    }

    public sealed class GeneratedFlowDelayScheduler : IDelayScheduler
    {
        public bool HasPending => _pendingCallback != null;

        private Action _pendingCallback;

        public IDisposable Schedule(float delaySeconds, Action callback)
        {
            _ = delaySeconds;
            _pendingCallback = callback;
            return new Cancellation(this, callback);
        }

        public void FirePending()
        {
            Action callback = _pendingCallback;
            _pendingCallback = null;
            callback?.Invoke();
        }

        private void Cancel(Action callback)
        {
            if (ReferenceEquals(_pendingCallback, callback))
            {
                _pendingCallback = null;
            }
        }

        private sealed class Cancellation : IDisposable
        {
            private GeneratedFlowDelayScheduler _scheduler;
            private readonly Action _callback;

            public Cancellation(GeneratedFlowDelayScheduler scheduler, Action callback)
            {
                _scheduler = scheduler;
                _callback = callback;
            }

            public void Dispose()
            {
                if (_scheduler == null)
                {
                    return;
                }

                _scheduler.Cancel(_callback);
                _scheduler = null;
            }
        }
    }

    internal sealed class GeneratedFlowInputService : IInputService
    {
        event Action<Vector2> IInputService.Pressed
        {
            add
            {
            }
            remove
            {
            }
        }

        event Action IInputService.BackPressed
        {
            add
            {
            }
            remove
            {
            }
        }

        public void EnablePlayerPress()
        {
        }

        public void DisablePlayerPress()
        {
        }
    }

    internal sealed class GeneratedFlowBoardView : IBoardView
    {
        public Vector3 ToLocalPoint(Vector3 worldPoint)
        {
            return worldPoint;
        }

        public void ShowMark(CellCoordinate coordinate, Mark mark)
        {
        }

        public void Clear()
        {
        }
    }

    internal sealed class GeneratedFlowCameraService : ICameraService
    {
        public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
        {
            return new Vector3(screenPoint.x, screenPoint.y, 0f);
        }
    }

    internal sealed class GeneratedFlowRandomService : IRandomService
    {
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return minimumInclusive;
        }

        public float Range(float minimumInclusive, float maximumInclusive)
        {
            return minimumInclusive;
        }
    }
}
#endif
