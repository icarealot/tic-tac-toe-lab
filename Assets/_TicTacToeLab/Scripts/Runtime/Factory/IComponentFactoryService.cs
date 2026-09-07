

using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IComponentFactoryService
    {
        public T Get<T>() where T : Component;
        public T Get<T>(Transform parent) where T : Component;
        public Component Get(Type componentType, Transform parent);
        public void Return<T>(T instance) where T : Component;
    }
}
