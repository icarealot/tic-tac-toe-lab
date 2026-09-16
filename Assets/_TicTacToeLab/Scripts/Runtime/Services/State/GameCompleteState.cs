namespace TicTacToeLab.Runtime
{
    public sealed class GameCompleteState : IAppState
    {
        public const float OUTCOME_PRESENTATION_DELAY_SECONDS = 1f;

        private readonly IBoardSession _boardSession;
        private readonly IStateMachine _stateMachine;
        private readonly IUIService _uiService;
        private readonly ICoroutineService _coroutineService;
        private readonly IInputService _inputService;

        private CoroutineHandle _pendingPresentation;
        private Outcome _capturedOutcome;
        private Mark _capturedTurn;
        private bool _hasPresentedOutcome;
        private bool _hasAcknowledged;

        public GameCompleteState(
            IBoardSession boardSession,
            IStateMachine stateMachine,
            IUIService uiService,
            ICoroutineService coroutineService,
            IInputService inputService)
        {
            _boardSession = boardSession;
            _stateMachine = stateMachine;
            _uiService = uiService;
            _coroutineService = coroutineService;
            _inputService = inputService;
        }

        public void Enter()
        {
            _hasPresentedOutcome = false;
            _hasAcknowledged = false;
            _inputService.DisablePlayerPress();
            _capturedOutcome = _boardSession.Outcome;
            _capturedTurn = _boardSession.Turn;
            _pendingPresentation = _coroutineService.RunAfter(OUTCOME_PRESENTATION_DELAY_SECONDS, PresentOutcomePopup);
        }

        public void Leave()
        {
            _pendingPresentation?.Dispose();
            _pendingPresentation = null;

            _uiService.CloseAllPopups();
            _inputService.EnablePlayerPress();
        }

        public void Back()
        {
            if (_pendingPresentation != null)
            {
                return;
            }

            EnterMainMenu();
        }

        private void PresentOutcomePopup()
        {
            if (_hasPresentedOutcome || _hasAcknowledged)
            {
                return;
            }

            _hasPresentedOutcome = true;
            _pendingPresentation = null;
            _uiService.ShowPopup<IOutcomePopup>(popup => popup.Setup(_capturedOutcome, _capturedTurn, EnterMainMenu));
        }

        private void EnterMainMenu()
        {
            if (_hasAcknowledged)
            {
                return;
            }

            _hasAcknowledged = true;
            _stateMachine.ChangeState<MainMenuState>();
        }
    }
}
