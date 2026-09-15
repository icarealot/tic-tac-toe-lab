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
            FakeInputService fakeInputService = new();
            AppStateMachine sut = new(fakeInputService);

            Assert.That(() => sut.ChangeState<UnregisteredSpyState>(),
                Throws.InvalidOperationException
                    .With.Message.Contains(nameof(UnregisteredSpyState)));
        }

        [Test]
        public void Changing_state_leaves_the_current_state_before_entering_the_next()
        {
            // Arrange
            List<string> log = new();
            FakeInputService fakeInputService = new();
            AppStateMachine sut = new(fakeInputService);
            sut.Add(new FirstSpyState(log));
            sut.Add(new SecondSpyState(log));
            sut.ChangeState<FirstSpyState>();
            log.Clear();

            // Act
            sut.ChangeState<SecondSpyState>();

            // Assert
            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave", "SecondSpyState.Enter" }));
        }

        [Test]
        public void After_a_transition_back_reaches_only_the_new_current_state()
        {
            // Arrange
            List<string> log = new();
            FakeInputService fakeInputService = new();
            AppStateMachine sut = new(fakeInputService);
            sut.Add(new FirstSpyState(log));
            sut.Add(new SecondSpyState(log));
            sut.ChangeState<FirstSpyState>();
            sut.ChangeState<SecondSpyState>();
            log.Clear();

            // Act
            fakeInputService.RaiseBack();

            // Assert
            Assert.That(log, Is.EqualTo(new[] { "SecondSpyState.Back" }));
        }

        [Test]
        public void Disposing_the_machine_leaves_the_current_state_and_detaches_back_input()
        {
            // Arrange
            List<string> log = new();
            FakeInputService fakeInputService = new();
            AppStateMachine sut = new(fakeInputService);
            sut.Add(new FirstSpyState(log));
            sut.ChangeState<FirstSpyState>();
            log.Clear();

            // Act
            sut.Dispose();

            // Assert
            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));

            // Act
            fakeInputService.RaiseBack();

            // Assert
            Assert.That(log, Is.EqualTo(new[] { "FirstSpyState.Leave" }));
        }
    }
}
