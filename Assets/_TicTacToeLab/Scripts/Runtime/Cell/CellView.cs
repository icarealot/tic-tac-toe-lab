using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CellView : MonoBehaviour
    {
        private MarkView _markView;

        public void ShowMark(IComponentFactoryService componentFactoryService, Mark mark)
        {
            _markView = componentFactoryService.Get<MarkView>(transform);
            _markView.transform.localPosition = Vector3.zero;
            _markView.Show(mark);
        }

        public void ClearMark(IComponentFactoryService componentFactoryService)
        {
            if (_markView == null)
            {
                return;
            }

            componentFactoryService.Return(_markView);
            _markView = null;
        }
    }
}
