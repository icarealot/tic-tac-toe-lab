

using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IFactoryService
    {
        public T Get<T>() where T : Component;
        public T Get<T>(Transform parent) where T : Component;
        public void Return<T>(T instance) where T : Component;
    }
}
