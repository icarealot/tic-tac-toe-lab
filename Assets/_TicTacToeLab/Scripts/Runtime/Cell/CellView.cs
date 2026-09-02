using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CellView : MonoBehaviour
    {
        private MarkView _markView;

        public void ShowMark(IFactoryService factoryService, Mark mark)
        {
            _markView = factoryService.Get<MarkView>(transform);
            _markView.transform.localPosition = Vector3.zero;
            _markView.Show(mark);
        }

        public void ClearMark(IFactoryService factoryService)
        {
            if (_markView == null)
            {
                return;
            }

            factoryService.Return(_markView);
            _markView = null;
        }
    }
}
