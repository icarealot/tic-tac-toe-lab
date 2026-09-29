using System;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public sealed class BoardPresenter : IDisposable
    {
        public event Action GameEnded;
        public event Action<Mark> TurnChanged;

        public Mark Turn => _boardModel.Turn;
        public Outcome Outcome => _boardModel.Outcome;

        private readonly BoardModel _boardModel;
        private readonly BoardLayout _boardLayout;
        private readonly IBoardView _boardView;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;

        public BoardPresenter(
            BoardModel boardModel,
            BoardLayout boardLayout,
            IBoardView boardView,
            IInputService inputService,
            ICameraService cameraService)
        {
            _boardModel = boardModel;
            _boardLayout = boardLayout;
            _boardView = boardView;
            _inputService = inputService;
            _cameraService = cameraService;

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

        public CellCoordinate SelectBotPlacement(IBot bot)
        {
            return bot.SelectPlacement(_boardModel);
        }

        public bool TryPlaceMark(CellCoordinate coordinate)
        {
            Mark mark = _boardModel.Turn;
            if (!_boardModel.TryPlaceMark(coordinate))
            {
                return false;
            }

            _boardView.ShowMark(coordinate, mark);

            if (_boardModel.Outcome == Outcome.InProgress)
            {
                TurnChanged?.Invoke(_boardModel.Turn);
                return true;
            }

            GameEnded?.Invoke();
            return true;
        }

        private void OnPressed(Vector2 screenPoint)
        {
            Vector3 worldPoint = _cameraService.ScreenToWorldPoint(screenPoint);
            Vector3 localPoint = _boardView.ToLocalPoint(worldPoint);

            if (!_boardLayout.TryResolvePoint(localPoint, out CellCoordinate coordinate))
            {
                return;
            }

            _ = TryPlaceMark(coordinate);
        }
    }
}
