using System;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeRandomChoiceSource : IRandomChoiceSource
    {
        public int Index { get; set; }
        public int LastCandidateCount { get; private set; }

        public int NextIndex(int candidateCount)
        {
            LastCandidateCount = candidateCount;
            return Math.Clamp(Index, 0, candidateCount - 1);
        }
    }
}
