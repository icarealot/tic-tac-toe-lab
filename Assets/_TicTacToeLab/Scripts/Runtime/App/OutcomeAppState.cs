using System;

namespace TicTacToeLab.Runtime
{
    internal sealed class OutcomeAppState : IAppState
    {
        private const float OUTCOME_PRESENTATION_DELAY_SECONDS = 1f;

        private readonly AppStateMachine _stateMachine;
        private readonly AppStateFactory _stateFactory;
        private readonly IAppUI _appUI;
        private readonly IDelayScheduler _delayScheduler;
        private readonly IInputService _inputService;
        private readonly Outcome _outcome;
        private readonly GameSetup _setup;

        private bool _isActive;
        private bool _isPresentationPending;
        private IDisposable _pendingPresentation;

        public OutcomeAppState(
            AppStateMachine stateMachine,
            AppStateFactory stateFactory,
            IAppUI appUI,
            IDelayScheduler delayScheduler,
            IInputService inputService,
            Outcome outcome,
            GameSetup setup)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
            _appUI = appUI;
            _delayScheduler = delayScheduler;
            _inputService = inputService;
            _outcome = outcome;
            _setup = setup;
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
            _appUI.Show<IOutcomeScreen>(screen => screen.Setup(_outcome, _setup, AcknowledgeOutcome));
        }

        private void AcknowledgeOutcome()
        {
            if (!_isActive || _isPresentationPending)
            {
                return;
            }

            _appUI.Close<IOutcomeScreen>();
            _stateMachine.TransitionFrom(this, _stateFactory.CreateHomeState());
        }
    }
}
