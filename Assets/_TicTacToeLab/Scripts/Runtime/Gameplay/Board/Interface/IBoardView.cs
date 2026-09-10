using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IBoardView
    {
        public void Construct(IFactoryService factoryService, int dimension, IReadOnlyList<CellPlacement> placements);
        public Vector3 ToLocalPoint(Vector3 worldPoint);
        public void ShowMark(int row, int column, Mark mark);
        public void Clear();
    }
}
