using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MarkView : MonoBehaviour, IMarkView
    {
        private SpriteRenderer SpriteRendererComponent
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                }

                return _spriteRenderer;
            }
        }

        [Header("Assets")]
        [SerializeField] private Sprite _oSprite;
        [SerializeField] private Sprite _xSprite;

        private SpriteRenderer _spriteRenderer;

        public void Show(Mark mark)
        {
            SpriteRendererComponent.sprite = mark switch
            {
                Mark.O => _oSprite,
                Mark.X => _xSprite,
                _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, "A mark view cannot show an undefined mark value.")
            };
        }
    }
}
