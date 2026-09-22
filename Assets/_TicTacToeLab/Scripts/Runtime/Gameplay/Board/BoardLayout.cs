using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardLayout
    {
        private const float CELL_SIZE = 1f;
        private const float CELL_SPACING = 0.2f;
        private const float CELL_STEP = CELL_SIZE + CELL_SPACING;

        private readonly int _dimension;
        private readonly float _centerOffset;

        public BoardLayout(int dimension)
        {
            _dimension = dimension;
            _centerOffset = (dimension - 1) * CELL_STEP * 0.5f;
        }

        public IReadOnlyList<CellPlacement> GetCellPlacements()
        {
            List<CellPlacement> placements = new(_dimension * _dimension);

            for (int row = 0; row < _dimension; row++)
            {
                for (int column = 0; column < _dimension; column++)
                {
                    CellCoordinate coordinate = new(row, column);
                    placements.Add(new CellPlacement(coordinate, GetCellLocalPoint(coordinate)));
                }
            }

            return placements;
        }

        public Vector3 GetCellLocalPoint(CellCoordinate coordinate)
        {
            return new Vector3(
                coordinate.Column * CELL_STEP - _centerOffset,
                _centerOffset - coordinate.Row * CELL_STEP,
                0f);
        }

        public bool TryResolvePoint(Vector3 localPoint, out CellCoordinate coordinate)
        {
            coordinate = default;

            if (!TryResolveAxis(localPoint.x, out int column))
            {
                return false;
            }

            if (!TryResolveAxis(-localPoint.y, out int row))
            {
                return false;
            }

            coordinate = new CellCoordinate(row, column);
            return true;
        }

        private bool TryResolveAxis(float localAxisPoint, out int axisIndex)
        {
            axisIndex = Mathf.RoundToInt((localAxisPoint + _centerOffset) / CELL_STEP);

            if (axisIndex < 0 || axisIndex >= _dimension)
            {
                return false;
            }

            float cellCenter = axisIndex * CELL_STEP - _centerOffset;
            return Mathf.Abs(localAxisPoint - cellCenter) <= CELL_SIZE * 0.5f;
        }
    }
}
