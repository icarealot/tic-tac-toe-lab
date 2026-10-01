namespace TicTacToeLab.EditModeTests
{
    public abstract class FakeScreen
    {
        public int PresentationCount { get; private set; }

        internal void RecordPresentation()
        {
            PresentationCount++;
        }
    }
}
