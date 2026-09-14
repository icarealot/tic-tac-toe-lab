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
            int stopCount = 0;
            CoroutineHandle handle = new(() => stopCount++);

            handle.Dispose();
            Assert.That(stopCount, Is.EqualTo(1));

            handle.Dispose();
            Assert.That(stopCount, Is.EqualTo(1));
        }

        [Test]
        public void Disposing_a_handle_marked_finished_does_not_stop_the_routine()
        {
            int stopCount = 0;
            CoroutineHandle handle = new(() => stopCount++);
            handle.MarkFinished();

            handle.Dispose();

            Assert.That(stopCount, Is.EqualTo(0));
        }

        [Test]
        public void A_handle_rejects_a_missing_stop_action()
        {
            Assert.That(() => _ = new CoroutineHandle(null), Throws.TypeOf<ArgumentNullException>());
        }
    }
}
