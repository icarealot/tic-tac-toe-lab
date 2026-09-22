#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class BoardPresenterHarness
    {
        public BoardPresenter Presenter { get; }

        private readonly BoardLayout _boardLayout;
        private readonly StubInputService _inputService = new();

        public BoardPresenterHarness()
        {
            BoardModel boardModel = new();
            _boardLayout = new BoardLayout(boardModel.Dimension);
            Presenter = new BoardPresenter(boardModel, _boardLayout, new StubBoardView(), _inputService, new StubCameraService());
        }

        public void RaisePress(CellCoordinate coordinate)
        {
            _inputService.RaisePress(_boardLayout.GetCellLocalPoint(coordinate));
        }

        private sealed class StubInputService : IInputService
        {
            public event Action<Vector2> Pressed;
            public event Action BackPressed;

            public void RaisePress(Vector2 screenPoint)
            {
                Pressed?.Invoke(screenPoint);
            }

            public void RaiseBack()
            {
                BackPressed?.Invoke();
            }

            public void EnablePlayerPress()
            {
            }

            public void DisablePlayerPress()
            {
            }
        }

        private sealed class StubCameraService : ICameraService
        {
            public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
            {
                return screenPoint;
            }
        }

        private sealed class StubBoardView : IBoardView
        {
            public void Construct(int dimension, IReadOnlyList<CellPlacement> placements)
            {
            }

            public Vector3 ToLocalPoint(Vector3 worldPoint)
            {
                return worldPoint;
            }

            public void ShowMark(CellCoordinate coordinate, Mark mark)
            {
            }

            public void Clear()
            {
            }
        }
    }
}
#endif
