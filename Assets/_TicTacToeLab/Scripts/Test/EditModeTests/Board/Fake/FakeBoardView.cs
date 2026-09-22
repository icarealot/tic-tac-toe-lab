using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeBoardView : IBoardView
    {
        public IReadOnlyList<(CellCoordinate Coordinate, Mark Mark)> ShownMarks => _shownMarks;
        public bool WasCleared { get; private set; }

        private readonly List<(CellCoordinate Coordinate, Mark Mark)> _shownMarks = new();

        public void Construct(int dimension, IReadOnlyList<CellPlacement> placements)
        {
        }

        public Vector3 ToLocalPoint(Vector3 worldPoint)
        {
            // A non-identity conversion proves the presenter applies the board-view boundary.
            return worldPoint * 0.5f;
        }

        public void ShowMark(CellCoordinate coordinate, Mark mark)
        {
            _shownMarks.Add((coordinate, mark));
        }

        public void Clear()
        {
            _shownMarks.Clear();
            WasCleared = true;
        }
    }
}
