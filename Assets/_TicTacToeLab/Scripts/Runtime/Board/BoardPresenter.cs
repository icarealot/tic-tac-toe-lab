using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToeLab.Runtime
{
    public class BoardPresenter : IDisposable
    {
        private readonly BoardModel _boardModel;
        private readonly IBoardView _boardView;
        private readonly IFactoryService _factoryService;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly ILogService _logService;

        public BoardPresenter(
            BoardModel boardModel,
            IBoardView boardView,
            IFactoryService factoryService,
            IInputService inputService,
            ICameraService cameraService,
            ILogService logService)
        {
            _boardModel = boardModel;
            _boardView = boardView;
            _factoryService = factoryService;
            _inputService = inputService;
            _cameraService = cameraService;
            _logService = logService;

            List<CellPlacement> placements = BuildCellPlacements(_boardModel.Dimension);
            _boardView.Construct(_factoryService, _boardModel.Dimension, placements);
            _inputService.Pressed += OnPressed;
        }

        public void Dispose()
        {
            _inputService.Pressed -= OnPressed;
        }

        private void OnPressed(Vector2 screenPoint)
        {
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
        }

        private List<CellPlacement> BuildCellPlacements(int dimension)
        {
            List<CellPlacement> placements = new(dimension * dimension);

            for (int row = 0; row < dimension; row++)
            {
                for (int column = 0; column < dimension; column++)
                {
                    placements.Add(new CellPlacement(row, column, _boardModel.GetCellLocalPoint(row, column)));
                }
            }

            return placements;
        }
    }
}
