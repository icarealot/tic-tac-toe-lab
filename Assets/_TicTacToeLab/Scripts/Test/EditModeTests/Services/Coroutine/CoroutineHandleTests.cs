using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class CoroutineHandleTests
    {
        [Test]
        public void Disposing_a_running_handle_stops_the_routine_once_and_a_second_disposal_does_nothing()
        {
            // Arrange
            int stopCount = 0;
            CoroutineHandle sut = new(() => stopCount++);

            // Act
            sut.Dispose();

            // Assert
            Assert.That(stopCount, Is.EqualTo(1));

            // Act
            sut.Dispose();

            // Assert
            Assert.That(stopCount, Is.EqualTo(1));
        }

        [Test]
        public void Disposing_a_handle_marked_finished_does_not_stop_the_routine()
        {
            int stopCount = 0;
            CoroutineHandle sut = new(() => stopCount++);
            sut.MarkFinished();

            sut.Dispose();

            Assert.That(stopCount, Is.EqualTo(0));
        }

        [Test]
        public void A_handle_rejects_a_missing_stop_action()
        {
            Assert.That(() => _ = new CoroutineHandle(null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
