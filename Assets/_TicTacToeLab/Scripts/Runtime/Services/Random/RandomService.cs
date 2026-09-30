using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class RandomService : IRandomService
    {
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return Random.Range(minimumInclusive, maximumExclusive);
        }

        public float Range(float minimumInclusive, float maximumInclusive)
        {
            return Random.Range(minimumInclusive, maximumInclusive);
        }
    }
}
