using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private MarkView _markViewPrefab;

        private MarkView _markView;

        public void Construct(CellPlacement placement)
        {
            transform.localPosition = placement.LocalPoint;
            CellCoordinate coordinate = placement.Coordinate;
            name = $"Cell ({coordinate.Row}, {coordinate.Column})";
        }

        public void ShowMark(Mark mark)
        {
            if (_markView != null)
            {
                return;
            }

            _markView = Instantiate(_markViewPrefab, transform);
            _markView.Show(mark);
        }

        public void ClearMark()
        {
            if (_markView == null)
            {
                return;
            }

            Destroy(_markView.gameObject);
            _markView = null;
        }
    }
}
