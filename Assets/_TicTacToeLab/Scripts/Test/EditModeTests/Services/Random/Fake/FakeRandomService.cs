using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeRandomService : IRandomService
    {
        public int IntegerResult { get; set; }
        public float FloatingPointResult { get; set; }
        public int LastIntegerMinimumInclusive { get; private set; }
        public int LastIntegerMaximumExclusive { get; private set; }
        public float LastFloatingPointMinimumInclusive { get; private set; }
        public float LastFloatingPointMaximumInclusive { get; private set; }
        public int IntegerCallCount { get; private set; }
        public int FloatingPointCallCount { get; private set; }

        public int Range(int minimumInclusive, int maximumExclusive)
        {
            LastIntegerMinimumInclusive = minimumInclusive;
            LastIntegerMaximumExclusive = maximumExclusive;
            IntegerCallCount++;
            return Math.Clamp(IntegerResult, minimumInclusive, maximumExclusive - 1);
        }

        public float Range(float minimumInclusive, float maximumInclusive)
        {
            LastFloatingPointMinimumInclusive = minimumInclusive;
            LastFloatingPointMaximumInclusive = maximumInclusive;
            FloatingPointCallCount++;
            return Math.Clamp(FloatingPointResult, minimumInclusive, maximumInclusive);
        }
    }
}
