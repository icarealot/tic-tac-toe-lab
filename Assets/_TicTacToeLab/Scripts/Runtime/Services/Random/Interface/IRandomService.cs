namespace TicTacToeLab.Runtime
{
    public interface IRandomService
    {
        public int Range(int minimumInclusive, int maximumExclusive);
        public float Range(float minimumInclusive, float maximumInclusive);
    }
}
