using System;

namespace TicTacToeLab.Runtime
{
    public interface IDelayScheduler
    {
        public IDisposable Schedule(float delaySeconds, Action callback);
    }
}
