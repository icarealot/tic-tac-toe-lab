#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    /// <summary>
    /// Retains the lifecycle probes of generated screens so isolated checks observe instances without searching the hierarchy.
    /// </summary>
    public sealed class GeneratedScreenRegistry : MonoBehaviour
    {
        private readonly List<LifecycleProbe> _probes = new();

        public void Register(LifecycleProbe probe)
        {
            _probes.Add(probe);
        }

        public void Unregister(LifecycleProbe probe)
        {
            _ = _probes.Remove(probe);
        }

        public LifecycleProbe LatestFor<T>() where T : Component
        {
            for (int index = _probes.Count - 1; index >= 0; index--)
            {
                LifecycleProbe probe = _probes[index];
                if (!probe.WasDestroyed && probe.GetComponent<T>() != null)
                {
                    return probe;
                }
            }

            return null;
        }

        public LifecycleProbe RequireLatestFor<T>() where T : Component
        {
            LifecycleProbe probe = LatestFor<T>();
            if (probe == null)
            {
                throw new InvalidOperationException($"No generated {typeof(T).Name} screen was observed.");
            }

            return probe;
        }
    }
}
#endif
