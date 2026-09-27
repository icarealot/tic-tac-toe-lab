using System;

namespace TicTacToeLab.Runtime
{
    internal sealed class OutcomeAppState : IAppState
    {
        private const float OUTCOME_PRESENTATION_DELAY_SECONDS = 1f;

        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly IApplicationUI _applicationUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;
        private readonly Outcome _outcome;

        private bool _isActive;
        private bool _isPresentationPending;
        private IDisposable _pendingPresentation;

        public OutcomeAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            IApplicationUI applicationUI,
            IDelayScheduler delayScheduler,
            IInputService inputService,
            Outcome outcome)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _applicationUI = applicationUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
            _outcome = outcome;
        }

        public void Enter()
        {
            _isActive = true;
            _inputService.DisablePlayerPress();
            _inputService.BackPressed += HandleBack;
            _isPresentationPending = true;
            _pendingPresentation = _delayScheduler.Schedule(OUTCOME_PRESENTATION_DELAY_SECONDS, PresentOutcome);
        }

        public void Exit()
        {
            _isActive = false;
            _isPresentationPending = false;
            _inputService.BackPressed -= HandleBack;

            if (_pendingPresentation != null)
            {
                _pendingPresentation.Dispose();
                _pendingPresentation = null;
            }
        }

        private void HandleBack()
        {
            if (!_isActive || _isPresentationPending)
            {
                return;
            }

            AcknowledgeOutcome();
        }

        private void PresentOutcome()
        {
            if (!_isActive || !_isPresentationPending)
            {
                return;
            }

            _pendingPresentation = null;
            _isPresentationPending = false;
            _applicationUI.ShowOutcome(_outcome, AcknowledgeOutcome);
        }

        private void AcknowledgeOutcome()
        {
            if (!_isActive || _isPresentationPending)
            {
                return;
            }

            _stateMachine.TransitionFrom(this, _stateFactory.CreateMainMenuState());
        }
    }
}
