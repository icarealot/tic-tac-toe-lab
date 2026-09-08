#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class RecordingFactoryService : IFactoryService
    {
        public RecordingFactoryService(IFactoryService factoryService)
        {
            _factoryService = factoryService;
        }

        public Component ReturnedInstance { get; private set; }

        public List<(Type Type, Transform Parent)> GetRequests { get; } = new();

        public T Get<T>() where T : class
        {
            GetRequests.Add((typeof(T), null));
            return _factoryService.Get<T>();
        }

        public T Get<T>(Transform parent) where T : class
        {
            GetRequests.Add((typeof(T), parent));
            return _factoryService.Get<T>(parent);
        }

        public void Return<T>(T instance) where T : class
        {
            ReturnedInstance = instance as Component;
            _factoryService.Return(instance);
        }

        private readonly IFactoryService _factoryService;
    }
}
#endif
