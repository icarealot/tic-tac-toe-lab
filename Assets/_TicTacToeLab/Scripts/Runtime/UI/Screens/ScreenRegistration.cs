using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    [Serializable]
    public sealed class ScreenRegistration
    {
        public MonoBehaviour Prefab => _prefab;
        public ScreenLayer Layer => _layer;

        [SerializeField] private MonoBehaviour _prefab;
        [SerializeField] private ScreenLayer _layer;

        public ScreenRegistration(MonoBehaviour prefab, ScreenLayer layer)
        {
            _prefab = prefab;
            _layer = layer;
        }
    }
}
