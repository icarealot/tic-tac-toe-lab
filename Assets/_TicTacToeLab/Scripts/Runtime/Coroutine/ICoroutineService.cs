using System;
using System.Collections;

namespace TicTacToeLab.Runtime
{
    public interface ICoroutineService
    {
        public CoroutineHandle Run(IEnumerator routine);
        public CoroutineHandle RunAfter(float delaySeconds, Action callback);
    }
}
