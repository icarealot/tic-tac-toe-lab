namespace TicTacToeLab.Runtime
{
    public interface IBot
    {
        public CellCoordinate SelectPlacement(BoardModel boardModel);
    }
}
