namespace TicTacToeLab.Runtime
{
    public interface ICellView
    {
        public void Construct(IFactoryService factoryService, CellPlacement placement);
        public void ShowMark(Mark mark);
        public void ClearMark();
    }
}
