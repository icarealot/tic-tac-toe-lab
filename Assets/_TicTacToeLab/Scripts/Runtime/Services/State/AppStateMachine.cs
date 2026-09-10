using System;
using System.Collections.Generic;

namespace TicTacToeLab.Runtime
{
    public sealed class AppStateMachine : IDisposable, IStateMachine
    {
        private readonly Dictionary<Type, IAppState> _states = new();
        private readonly IInputService _inputService;

        private IAppState _currentState;

        public AppStateMachine(IInputService inputService)
        {
            _inputService = inputService;
            _inputService.BackPressed += OnBackPressed;
        }

        public void Add<TState>(TState state) where TState : IAppState
        {
            _states[typeof(TState)] = state;
        }

        public void ChangeState<TState>() where TState : IAppState
        {
            if (!_states.TryGetValue(typeof(TState), out IAppState nextState))
            {
                throw new InvalidOperationException($"No state of type {typeof(TState).Name} has been added to the state machine.");
            }

            _currentState?.Leave();
            _currentState = nextState;
            _currentState.Enter();
        }

        public void Dispose()
        {
            _inputService.BackPressed -= OnBackPressed;
            _currentState?.Leave();
            _currentState = null;
        }

        private void OnBackPressed()
        {
            _currentState?.Back();
        }
    }
}
