using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public readonly struct CellPlacement
    {
        public int Row { get; }
        public int Column { get; }
        public Vector3 LocalPoint { get; }

        public CellPlacement(int row, int column, Vector3 localPoint)
        {
            Row = row;
            Column = column;
            LocalPoint = localPoint;
        }
    }
}
