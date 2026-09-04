using System;

namespace TicTacToeLab.Runtime
{
    public class BoardSession : IDisposable
    {
        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public Mark Turn => _boardPresenter.Turn;

        private readonly BoardPresenter _boardPresenter;

        public BoardSession(BoardPresenter boardPresenter)
        {
            _boardPresenter = boardPresenter;
            _boardPresenter.GameEnded += OnGameEnded;
            _boardPresenter.TurnChanged += OnTurnChanged;
        }

        public void Dispose()
        {
            _boardPresenter.GameEnded -= OnGameEnded;
            _boardPresenter.TurnChanged -= OnTurnChanged;
            _boardPresenter.Dispose();
        }

        public void Reset()
        {
            _boardPresenter.Reset();
        }

        private void OnGameEnded()
        {
            GameEnded?.Invoke();
        }

        private void OnTurnChanged(Mark turn)
        {
            TurnChanged?.Invoke(turn);
        }
    }
}
