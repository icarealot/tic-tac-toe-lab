using System;

namespace TicTacToeLab.Runtime
{
    public interface IHomeScreen : IScreen
    {
        public void Setup(Action onStart);
    }
}
