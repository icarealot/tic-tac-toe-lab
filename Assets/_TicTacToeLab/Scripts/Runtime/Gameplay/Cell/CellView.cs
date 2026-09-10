using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CellView : MonoBehaviour, ICellView
    {
        private IFactoryService _factoryService;
        private IMarkView _markView;

        public void Construct(IFactoryService factoryService, CellPlacement placement)
        {
            _factoryService = factoryService;
            transform.localPosition = placement.LocalPoint;
            name = $"Cell ({placement.Row}, {placement.Column})";
        }

        public void ShowMark(Mark mark)
        {
            _markView = _factoryService.Get<IMarkView>(transform);
            _markView.Show(mark);
        }

        public void ClearMark()
        {
            if (_markView == null)
            {
                return;
            }

            _factoryService.Return(_markView);
            _markView = null;
        }
    }
}
