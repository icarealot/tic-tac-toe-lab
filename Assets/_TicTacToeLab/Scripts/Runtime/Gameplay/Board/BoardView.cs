using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class BoardView : MonoBehaviour, IBoardView
    {
        [SerializeField] private CellView _cellViewPrefab;

        private CellView[,] _cellViews;

        public void Construct(int dimension, IReadOnlyList<CellPlacement> placements)
        {
            _cellViews = new CellView[dimension, dimension];

            foreach (CellPlacement placement in placements)
            {
                CellView cellView = Instantiate(_cellViewPrefab, transform);
                cellView.Construct(placement);

                CellCoordinate coordinate = placement.Coordinate;
                _cellViews[coordinate.Row, coordinate.Column] = cellView;
            }
        }

        public Vector3 ToLocalPoint(Vector3 worldPoint)
        {
            return transform.InverseTransformPoint(worldPoint);
        }

        public void ShowMark(CellCoordinate coordinate, Mark mark)
        {
            _cellViews[coordinate.Row, coordinate.Column].ShowMark(mark);
        }

        public void Clear()
        {
            foreach (CellView cellView in _cellViews)
            {
                cellView.ClearMark();
            }
        }
    }
}
