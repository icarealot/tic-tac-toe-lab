namespace TicTacToeLab.Runtime
{
    public interface IRandomChoiceSource
    {
        public int NextIndex(int candidateCount);
    }
}
