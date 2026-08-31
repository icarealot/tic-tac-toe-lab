using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardView : MonoBehaviour
    {
        private const int BOARD_DIMENSION = 3;
        private const float CELL_SIZE = 1f;
        private const float CELL_SPACING = 0.2f;
        private const float CELL_STEP = CELL_SIZE + CELL_SPACING;

        public void Construct(IFactoryService factoryService)
        {
            float centerOffset = (BOARD_DIMENSION - 1) * CELL_STEP * 0.5f;

            for (int row = 0; row < BOARD_DIMENSION; row++)
            {
                for (int column = 0; column < BOARD_DIMENSION; column++)
                {
                    CellView cellView = factoryService.Get<CellView>(transform);
                    cellView.transform.localPosition = new Vector3(
                        column * CELL_STEP - centerOffset,
                        centerOffset - row * CELL_STEP,
                        0f);
                    cellView.transform.localScale = Vector3.one * CELL_SIZE;
                    cellView.name = $"Cell ({row}, {column})";
                }
            }
        }
    }
}
