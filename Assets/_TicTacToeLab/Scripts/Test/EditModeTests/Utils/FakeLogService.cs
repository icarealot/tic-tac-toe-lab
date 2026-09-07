using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class FakeLogService : ILogService
    {
        public List<string> Messages { get; } = new();
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();

        public void Log(string message)
        {
            Messages.Add(message);
        }

        public void LogWarning(string message)
        {
            Warnings.Add(message);
        }

        public void LogError(string message)
        {
            Errors.Add(message);
        }
    }
}
