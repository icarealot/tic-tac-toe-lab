using System;

namespace TicTacToeLab.Runtime
{
    public sealed class PveTurnController : IGameplayTurnController
    {
        private const float BOT_TURN_MINIMUM_DELAY_SECONDS = 0.4f;
        private const float BOT_TURN_MAXIMUM_DELAY_SECONDS = 1f;

        private readonly BoardPresenter _boardPresenter;
        private readonly IInputService _inputService;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IRandomService _randomService;
        private readonly IBot _bot;

        private TurnLifecycle _lifecycle;
        private IDisposable _pendingBotPlacementCancellation;
        private Action _pendingBotPlacementCallback;

        public PveTurnController(
            BoardPresenter boardPresenter,
            IInputService inputService,
            IDelayScheduler delayScheduler,
            IRandomService randomService,
            IBot bot)
        {
            _boardPresenter = boardPresenter;
            _inputService = inputService;
            _delayScheduler = delayScheduler;
            _randomService = randomService;
            _bot = bot;
        }

        public void Enter()
        {
            if (_lifecycle != TurnLifecycle.Constructed)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Active;
            _boardPresenter.TurnChanged += HandleTurnChanged;
            SynchronizeWithBoard();
        }

        public void Pause()
        {
            if (_lifecycle != TurnLifecycle.Active)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Paused;
            CancelPendingBotPlacement();
            _inputService.DisablePlayerPress();
        }

        public void Resume()
        {
            if (_lifecycle != TurnLifecycle.Paused)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Active;
            SynchronizeWithBoard();
        }

        public void Exit()
        {
            if (_lifecycle == TurnLifecycle.Exited)
            {
                return;
            }

            _lifecycle = TurnLifecycle.Exited;
            CancelPendingBotPlacement();
            _boardPresenter.TurnChanged -= HandleTurnChanged;
            _inputService.DisablePlayerPress();
        }

        private void SynchronizeWithBoard()
        {
            if (_boardPresenter.Outcome != Outcome.InProgress)
            {
                _inputService.DisablePlayerPress();
                return;
            }

            if (_boardPresenter.Turn == Mark.O)
            {
                ScheduleBotPlacement();
                return;
            }

            _inputService.EnablePlayerPress();
        }

        private void HandleTurnChanged(Mark turn)
        {
            if (_lifecycle != TurnLifecycle.Active
                || _boardPresenter.Outcome != Outcome.InProgress)
            {
                return;
            }

            if (turn == Mark.O)
            {
                ScheduleBotPlacement();
                return;
            }

            _inputService.EnablePlayerPress();
        }

        private void ScheduleBotPlacement()
        {
            if (_pendingBotPlacementCallback != null
                || _lifecycle != TurnLifecycle.Active
                || _boardPresenter.Outcome != Outcome.InProgress
                || _boardPresenter.Turn != Mark.O)
            {
                return;
            }

            _inputService.DisablePlayerPress();
            float delaySeconds = _randomService.Range(
                BOT_TURN_MINIMUM_DELAY_SECONDS,
                BOT_TURN_MAXIMUM_DELAY_SECONDS);
            Action botPlacementCallback = null;
            botPlacementCallback = () => PlaceBotMark(botPlacementCallback);
            _pendingBotPlacementCallback = botPlacementCallback;
            _pendingBotPlacementCancellation = _delayScheduler.Schedule(delaySeconds, botPlacementCallback);
        }

        private void PlaceBotMark(Action botPlacementCallback)
        {
            if (!ReferenceEquals(_pendingBotPlacementCallback, botPlacementCallback))
            {
                return;
            }

            CancelPendingBotPlacement();

            if (_lifecycle != TurnLifecycle.Active
                || _boardPresenter.Outcome != Outcome.InProgress
                || _boardPresenter.Turn != Mark.O)
            {
                return;
            }

            CellCoordinate coordinate = _boardPresenter.SelectBotPlacement(_bot);
            _ = _boardPresenter.TryPlaceMark(coordinate);
        }

        private void CancelPendingBotPlacement()
        {
            _pendingBotPlacementCallback = null;
            IDisposable cancellation = _pendingBotPlacementCancellation;
            _pendingBotPlacementCancellation = null;

            if (cancellation != null)
            {
                cancellation.Dispose();
            }
        }
    }
}
