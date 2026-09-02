using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeLogService : ILogService
    {
        public List<string> Messages { get; } = new();

        public void Log(string message)
        {
            Messages.Add(message);
        }

        public void LogWarning(string message)
        {
        }

        public void LogError(string message)
        {
        }
    }
}
