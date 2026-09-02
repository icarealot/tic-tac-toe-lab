using System;

namespace TicTacToeLab.Runtime
{
    public class BoardSession : IDisposable
    {
        public event Action GameEnded;

        private readonly BoardPresenter _boardPresenter;

        public BoardSession(BoardPresenter boardPresenter)
        {
            _boardPresenter = boardPresenter;
            _boardPresenter.GameEnded += OnGameEnded;
        }

        public void Dispose()
        {
            _boardPresenter.GameEnded -= OnGameEnded;
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
    }
}
