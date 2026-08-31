using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CellView : MonoBehaviour
    {
        public void ShowMark(IFactoryService factoryService, Mark mark)
        {
            MarkView markView = factoryService.Get<MarkView>(transform);
            markView.transform.localPosition = Vector3.zero;
            markView.Show(mark);
        }
    }
}
