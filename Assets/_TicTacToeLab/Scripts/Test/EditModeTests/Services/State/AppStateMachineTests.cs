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
            public FirstSpyState(List<string> log) : base(log)
            {
            }
        }

        private sealed class SecondSpyState : SpyState
        {
            public SecondSpyState(List<string> log) : base(log)
            {
            }
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
        public void Disposing_the_machine_leaves_the_current_state()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _log.Clear();

            _stateMachine.Dispose();

            Assert.That(_log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));
        }

        [Test]
        public void A_raised_back_reaches_the_current_state_and_no_other()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _log.Clear();

            _fakeInputService.RaiseBack();

            Assert.That(_log, Is.EqualTo(new[] { "FirstSpyState.Back" }));
        }

        [Test]
        public void After_a_transition_back_reaches_the_new_state_and_not_the_one_just_left()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _stateMachine.ChangeState<SecondSpyState>();
            _log.Clear();

            _fakeInputService.RaiseBack();

            Assert.That(_log, Is.EqualTo(new[] { "SecondSpyState.Back" }));
        }

        [Test]
        public void After_disposal_back_reaches_nothing()
        {
            _stateMachine.ChangeState<FirstSpyState>();
            _stateMachine.Dispose();
            _log.Clear();

            _fakeInputService.RaiseBack();

            Assert.That(_log, Is.Empty);
        }
    }
}
