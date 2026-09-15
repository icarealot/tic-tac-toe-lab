using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardPresenter : IDisposable
    {
        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public Mark Turn => _boardModel.Turn;
        public Outcome Outcome => _boardModel.Outcome;

        private readonly BoardModel _boardModel;
        private readonly IBoardView _boardView;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly ILogService _logService;

        public BoardPresenter(
            BoardModel boardModel,
            IBoardView boardView,
            IInputService inputService,
            ICameraService cameraService,
            ILogService logService)
        {
            _boardModel = boardModel;
            _boardView = boardView;
            _inputService = inputService;
            _cameraService = cameraService;
            _logService = logService;

            _inputService.Pressed += OnPressed;
        }

        public void Dispose()
        {
            _inputService.Pressed -= OnPressed;
        }

        public void Reset()
        {
            _boardModel.Reset();
            _boardView.Clear();
        }

        private void OnPressed(Vector2 screenPoint)
        {
            if (_boardModel.Outcome != Outcome.InProgress)
            {
                _logService.Log("Rejected press: the game is over");
                return;
            }

            Vector3 worldPoint = _cameraService.ScreenToWorldPoint(screenPoint);
            Vector3 localPoint = _boardView.ToLocalPoint(worldPoint);

            if (!_boardModel.TryResolveCell(localPoint, out int row, out int column))
            {
                return;
            }

            if (!_boardModel.IsEmpty(row, column))
            {
                _logService.Log($"Rejected press on occupied cell ({row}, {column})");
                return;
            }

            _boardView.ShowMark(row, column, _boardModel.Turn);
            _boardModel.PlaceMark(row, column);
            TurnChanged?.Invoke(_boardModel.Turn);

            if (_boardModel.Outcome == Outcome.Win)
            {
                _logService.Log($"{_boardModel.Turn} wins");
            }
            else if (_boardModel.Outcome == Outcome.Draw)
            {
                _logService.Log("Draw");
            }

            if (_boardModel.Outcome != Outcome.InProgress)
            {
                GameEnded?.Invoke();
            }
        }
    }
}
