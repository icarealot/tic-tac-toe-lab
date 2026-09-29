using System;

namespace TicTacToeLab.Runtime
{
    public sealed class AppStateMachine : IDisposable
    {
        private readonly AppStateFactory _stateFactory;

        private IAppState _currentState;
        private bool _isDisposed;

        public AppStateMachine(
            BoardPresenter boardPresenter,
            IAppUI appUI,
            IDelayScheduler delayScheduler,
            IInputService inputService,
            IRandomChoiceSource randomChoiceSource)
        {
            _stateFactory = new AppStateFactory(
                this,
                boardPresenter,
                appUI,
                delayScheduler,
                inputService,
                randomChoiceSource);
        }

        public void Start()
        {
            if (_isDisposed)
            {
                return;
            }

            TransitionFrom(null, _stateFactory.CreateHomeState());
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            IAppState currentState = _currentState;
            _currentState = null;

            if (currentState != null)
            {
                currentState.Exit();
            }
        }

        internal void TransitionFrom(IAppState sourceState, IAppState destinationState)
        {
            if (_isDisposed || !ReferenceEquals(_currentState, sourceState))
            {
                return;
            }

            IAppState previousState = _currentState;
            _currentState = null;

            if (previousState != null)
            {
                previousState.Exit();
            }

            _currentState = destinationState;
            destinationState.Enter();
        }
    }
}
