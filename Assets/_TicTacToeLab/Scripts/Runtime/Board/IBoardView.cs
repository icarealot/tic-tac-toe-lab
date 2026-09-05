using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IBoardView
    {
        public Vector3 ToLocalPoint(Vector3 worldPoint);
        public void ShowMark(int row, int column, Mark mark);
        public void Clear();
    }
}
