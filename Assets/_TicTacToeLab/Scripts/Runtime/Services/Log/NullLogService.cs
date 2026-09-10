namespace TicTacToeLab.Runtime
{
    public class NullLogService : ILogService
    {
        public void Log(string message)
        {
        }

        public void LogWarning(string message)
        {
        }

        public void LogError(string message)
        {
        }
    }
}
