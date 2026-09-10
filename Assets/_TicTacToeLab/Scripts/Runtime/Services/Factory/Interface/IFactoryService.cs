using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public interface IFactoryService
    {
        public T Get<T>() where T : class;
        public T Get<T>(Transform parent) where T : class;
        public void Return<T>(T instance) where T : class;
    }
}
