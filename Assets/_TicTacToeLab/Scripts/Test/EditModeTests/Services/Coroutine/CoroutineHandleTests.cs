using System;
using NUnit.Framework;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    /// <summary>
    /// CoroutineHandle state transitions asserted on plain objects: disposing a running handle
    /// stops the routine exactly once and repeated disposal is a no-op, while disposing a finished
    /// handle never stops it. The constructor's rejection of a missing stop action is part of the
    /// handle contract: a handle that cannot stop cannot fulfil cancellation. These lifecycle
    /// rules need no frames, so EditMode is the smallest sufficient fixture; real frame
    /// continuation and delayed execution belong to the isolated PlayMode component in
    /// CoroutineServiceTests.
    /// </summary>
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
