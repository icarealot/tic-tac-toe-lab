using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public readonly struct CellPlacement
    {
        public CellCoordinate Coordinate { get; }
        public Vector3 LocalPoint { get; }

        public CellPlacement(CellCoordinate coordinate, Vector3 localPoint)
        {
            Coordinate = coordinate;
            LocalPoint = localPoint;
        }
    }
}
