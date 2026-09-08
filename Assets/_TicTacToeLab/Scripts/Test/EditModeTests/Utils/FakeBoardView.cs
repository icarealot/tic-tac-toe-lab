using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeBoardView : IBoardView
    {
        public List<(int Row, int Column, Mark Mark)> ShownMarks { get; } = new();
        public bool WasCleared { get; private set; }
        public int ClearCount { get; private set; }

        public void Construct(IFactoryService factoryService, int dimension, IReadOnlyList<CellPlacement> placements)
        {
        }

        public Vector3 ToLocalPoint(Vector3 worldPoint)
        {
            return worldPoint;
        }

        public void ShowMark(int row, int column, Mark mark)
        {
            ShownMarks.Add((row, column, mark));
        }

        public void Clear()
        {
            WasCleared = true;
            ClearCount++;
        }
    }
}
