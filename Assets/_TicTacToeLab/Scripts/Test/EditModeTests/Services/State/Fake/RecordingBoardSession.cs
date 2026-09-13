using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;

namespace TicTacToeLab.EditModeTests
{
    public sealed class RecordingBoardSession : IBoardSession
    {
        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public Mark Turn => Mark.X;

        private readonly List<string> _log;

        public RecordingBoardSession(List<string> log)
        {
            _log = log;
        }

        public void Reset()
        {
            _log.Add("BoardSession.Reset");
        }
    }
}
