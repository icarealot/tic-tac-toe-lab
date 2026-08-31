using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MarkView : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField] private Sprite _oSprite;
        [SerializeField] private Sprite _xSprite;

        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void Show(Mark mark)
        {
            _spriteRenderer.sprite = mark switch
            {
                Mark.O => _oSprite,
                Mark.X => _xSprite,
                _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, "A mark view cannot show an empty mark.")
            };
        }
    }
}
