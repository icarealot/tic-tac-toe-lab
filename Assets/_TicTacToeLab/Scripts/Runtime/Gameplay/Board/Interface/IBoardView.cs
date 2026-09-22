using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IBoardView
    {
        public void Construct(int dimension, IReadOnlyList<CellPlacement> placements);
        public Vector3 ToLocalPoint(Vector3 worldPoint);
        public void ShowMark(CellCoordinate coordinate, Mark mark);
        public void Clear();
    }
}
