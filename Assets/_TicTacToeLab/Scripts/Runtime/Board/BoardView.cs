using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardView : MonoBehaviour, IBoardView
    {
        private IFactoryService _factoryService;
        private CellView[,] _cellViews;

        public void Construct(IFactoryService factoryService, int dimension, IReadOnlyList<CellPlacement> placements)
        {
            _factoryService = factoryService;
            _cellViews = new CellView[dimension, dimension];

            foreach (CellPlacement placement in placements)
            {
                CellView cellView = factoryService.Get<CellView>(transform);
                cellView.transform.localPosition = placement.LocalPoint;
                cellView.name = $"Cell ({placement.Row}, {placement.Column})";

                _cellViews[placement.Row, placement.Column] = cellView;
            }
        }

        public Vector3 ToLocalPoint(Vector3 worldPoint)
        {
            return transform.InverseTransformPoint(worldPoint);
        }

        public void ShowMark(int row, int column, Mark mark)
        {
            _cellViews[row, column].ShowMark(_factoryService, mark);
        }

        public void Clear()
        {
            foreach (CellView cellView in _cellViews)
            {
                cellView.ClearMark(_factoryService);
            }
        }
    }
}
