using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class UnityRandomChoiceSource : IRandomChoiceSource
    {
        public int NextIndex(int candidateCount)
        {
            return Random.Range(0, candidateCount);
        }
    }
}
