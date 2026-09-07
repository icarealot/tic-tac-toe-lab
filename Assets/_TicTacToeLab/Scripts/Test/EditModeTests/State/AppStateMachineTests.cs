using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class AppStateMachineTests
    {
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
            List<string> log = new();
            AppStateMachine stateMachine = new(new FakeInputService());
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.Add(new SecondSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            stateMachine.ChangeState<SecondSpyState>();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave", "SecondSpyState.Enter" }));
        }

        [Test]
        public void Disposing_the_machine_leaves_the_current_state()
        {
            List<string> log = new();
            AppStateMachine stateMachine = new(new FakeInputService());
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            stateMachine.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));
        }

        [Test]
        public void A_raised_back_reaches_the_current_state_and_no_other()
        {
            List<string> log = new();
            FakeInputService fakeInputService = new();
            AppStateMachine stateMachine = new(fakeInputService);
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.Add(new SecondSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            log.Clear();

            fakeInputService.RaiseBack();

            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Back" }));
        }

        [Test]
        public void After_a_transition_back_reaches_the_new_state_and_not_the_one_just_left()
        {
            List<string> log = new();
            FakeInputService fakeInputService = new();
            AppStateMachine stateMachine = new(fakeInputService);
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.Add(new SecondSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            stateMachine.ChangeState<SecondSpyState>();
            log.Clear();

            fakeInputService.RaiseBack();

            Assert.That(log, Is.EqualTo(new[] { "SecondSpyState.Back" }));
        }

        [Test]
        public void After_disposal_back_reaches_nothing()
        {
            List<string> log = new();
            FakeInputService fakeInputService = new();
            AppStateMachine stateMachine = new(fakeInputService);
            stateMachine.Add(new FirstSpyState(log));
            stateMachine.ChangeState<FirstSpyState>();
            stateMachine.Dispose();
            log.Clear();

            fakeInputService.RaiseBack();

            Assert.That(log, Is.Empty);
        }
    }
}
