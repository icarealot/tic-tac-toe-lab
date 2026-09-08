using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardView : MonoBehaviour, IBoardView
    {
        private ICellView[,] _cellViews;

        public void Construct(IFactoryService factoryService, int dimension, IReadOnlyList<CellPlacement> placements)
        {
            _cellViews = new ICellView[dimension, dimension];

            foreach (CellPlacement placement in placements)
            {
                ICellView cellView = factoryService.Get<ICellView>(transform);
                cellView.Construct(factoryService, placement);

                _cellViews[placement.Row, placement.Column] = cellView;
            }
        }

        public Vector3 ToLocalPoint(Vector3 worldPoint)
        {
            return transform.InverseTransformPoint(worldPoint);
        }

        public void ShowMark(int row, int column, Mark mark)
        {
            _cellViews[row, column].ShowMark(mark);
        }

        public void Clear()
        {
            foreach (ICellView cellView in _cellViews)
            {
                cellView.ClearMark();
            }
        }
    }
}
