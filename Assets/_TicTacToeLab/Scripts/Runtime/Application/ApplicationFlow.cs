using System;

namespace TicTacToeLab.Runtime
{
    public sealed class ApplicationFlow : IDisposable
    {
        private readonly ApplicationStateFactory _stateFactory;

        private IApplicationState _currentState;
        private bool _isDisposed;

        public ApplicationFlow(
            BoardPresenter boardPresenter,
            IApplicationUI applicationUI,
            IDelayScheduler delayScheduler,
            IInputService inputService)
        {
            _stateFactory = new ApplicationStateFactory(this, boardPresenter, applicationUI, delayScheduler, inputService);
        }

        public void Start()
        {
            if (_isDisposed)
            {
                return;
            }

            TransitionFrom(null, _stateFactory.CreateMainMenuState());
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            IApplicationState currentState = _currentState;
            _currentState = null;

            if (currentState != null)
            {
                currentState.Exit();
            }
        }

        internal void TransitionFrom(IApplicationState sourceState, IApplicationState destinationState)
        {
            if (_isDisposed || !ReferenceEquals(_currentState, sourceState))
            {
                return;
            }

            IApplicationState previousState = _currentState;
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
