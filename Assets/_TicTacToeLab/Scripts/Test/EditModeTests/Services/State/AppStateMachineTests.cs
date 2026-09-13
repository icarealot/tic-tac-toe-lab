using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class AppStateMachineTests
    {
        private List<string> _log;
        private FakeInputService _fakeInputService;
        private AppStateMachine _stateMachine;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _fakeInputService = new FakeInputService();
            _stateMachine = new AppStateMachine(_fakeInputService);
            _stateMachine.Add(new FirstSpyState(_log));
            _stateMachine.Add(new SecondSpyState(_log));
        }

        private abstract class SpyState : IAppState
        {
            protected readonly List<string> Log;

            protected SpyState(List<string> log)
            {
                Log = log;
            }

            public void Enter()
            {
                Log.Add($"{GetType().Name}.Enter");
            }

            public void Leave()
            {
                Log.Add($"{GetType().Name}.Leave");
            }

            public void Back()
            {
                Log.Add($"{GetType().Name}.Back");
            }
        }

        private sealed class FirstSpyState : SpyState
        {
            public FirstSpyState(List<string> log) : base(log) { }
        }

        private sealed class SecondSpyState : SpyState
        {
            public SecondSpyState(List<string> log) : base(log) { }
        }

        private sealed class UnregisteredSpyState : SpyState
        {
            public UnregisteredSpyState(List<string> log) : base(log) { }
        }

        [Test]
        public void Transitioning_to_a_state_type_that_was_never_added_throws()
        {
            Assert.That(() => _stateMachine.ChangeState<UnregisteredSpyState>(),
                Throws.InvalidOperationException
                    .With.Message.Contains(nameof(UnregisteredSpyState)));
        }

        [Test]
        public void Changing_state_leaves_the_current_state_before_entering_the_next()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _log.Clear();

            _stateMachine.ChangeState<SecondSpyState>();

            Assert.That(_log, Is.EqualTo(new[] { "FirstSpyState.Leave", "SecondSpyState.Enter" }));
        }

        [Test]
        public void After_a_transition_back_reaches_only_the_new_current_state()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _stateMachine.ChangeState<SecondSpyState>();
            _log.Clear();

            _fakeInputService.RaiseBack();

            Assert.That(_log, Is.EqualTo(new[] { "SecondSpyState.Back" }));
        }

        [Test]
        public void Disposing_the_machine_leaves_the_current_state_and_detaches_back_input()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _log.Clear();

            _stateMachine.Dispose();

            Assert.That(_log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));

            _fakeInputService.RaiseBack();

            Assert.That(_log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));
        }
    }
}
