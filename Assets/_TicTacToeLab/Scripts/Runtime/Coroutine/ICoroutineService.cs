using System.Collections;

namespace TicTacToeLab.Runtime
{
    public interface ICoroutineService
    {
        public CoroutineHandle Run(IEnumerator routine);
    }
}
