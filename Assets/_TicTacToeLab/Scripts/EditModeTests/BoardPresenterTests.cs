using System;
using System.Collections.Generic;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;

namespace TicTacToeLab.EditModeTests
{
    public sealed class BoardPresenterTests
    {
        private sealed class FakeBoardView : IBoardView
        {
            public int Dimension { get; private set; }
            public IReadOnlyList<CellPlacement> Placements { get; private set; }
            public List<(int Row, int Column, Mark Mark)> ShownMarks { get; } = new();

            public void Construct(IFactoryService factoryService, int dimension, IReadOnlyList<CellPlacement> placements)
            {
                Dimension = dimension;
                Placements = placements;
            }

            public Vector3 ToLocalPoint(Vector3 worldPoint)
            {
                return worldPoint;
            }

            public void ShowMark(int row, int column, Mark mark)
            {
                ShownMarks.Add((row, column, mark));
            }
        }

        private sealed class FakeInputService : IInputService
        {
            public event Action<Vector2> Pressed;

            public void RaisePress(Vector2 screenPoint)
            {
                Pressed?.Invoke(screenPoint);
            }
        }

        private sealed class FakeCameraService : ICameraService
        {
            public Vector3 ScreenToWorldPoint(Vector2 screenPoint)
            {
                return new Vector3(screenPoint.x, screenPoint.y, 0f);
            }
        }

        private sealed class FakeLogService : ILogService
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

        private static BoardPresenter CreatePresenter(
            BoardModel boardModel,
            FakeBoardView boardView,
            FakeInputService inputService,
            FakeLogService logService)
        {
            return new BoardPresenter(
                boardModel, boardView, factoryService: null,
                inputService, new FakeCameraService(), logService);
        }

        [Test]
        public void Initializing_the_presenter_delivers_exactly_nine_placements_one_per_cell_coordinate()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            _ = CreatePresenter(boardModel, fakeBoardView, new FakeInputService(), new FakeLogService());

            Assert.That(fakeBoardView.Placements, Has.Count.EqualTo(9));

            for (int row = 0; row < boardModel.Dimension; row++)
            {
                for (int column = 0; column < boardModel.Dimension; column++)
                {
                    bool found = false;
                    foreach (CellPlacement placement in fakeBoardView.Placements)
                    {
                        if (placement.Row != row || placement.Column != column)
                        {
                            continue;
                        }

                        found = true;
                        Assert.That(placement.LocalPoint, Is.EqualTo(boardModel.GetCellLocalPoint(row, column)));
                    }

                    Assert.That(found, Is.True, $"No placement was delivered for cell ({row}, {column}).");
                }
            }
        }

        [Test]
        public void A_press_on_an_empty_cell_shows_an_O_in_that_cell_and_the_model_records_it()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());
            Vector3 cellCenter = boardModel.GetCellLocalPoint(0, 2);
            fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));

            Assert.That(fakeBoardView.ShownMarks, Is.EqualTo(new[] { (0, 2, Mark.O) }));
            Assert.That(boardModel.GetMark(0, 2), Is.EqualTo(Mark.O));
        }

        [Test]
        public void A_press_on_a_cell_already_holding_a_mark_shows_nothing_and_leaves_the_existing_mark_alone()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());
            Vector3 cellCenter = boardModel.GetCellLocalPoint(1, 1);
            fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));
            fakeBoardView.ShownMarks.Clear();

            fakeInputService.RaisePress(new Vector2(cellCenter.x, cellCenter.y));

            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
            Assert.That(boardModel.GetMark(1, 1), Is.EqualTo(Mark.O));
        }

        [Test]
        public void A_press_resolving_to_no_cell_shows_nothing()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());
            fakeInputService.RaisePress(new Vector2(0.55f, 0f));

            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
        }

        [Test]
        public void A_press_off_the_board_shows_nothing()
        {
            BoardModel boardModel = new();
            FakeBoardView fakeBoardView = new();
            FakeInputService fakeInputService = new();
            _ = CreatePresenter(boardModel, fakeBoardView, fakeInputService, new FakeLogService());
            fakeInputService.RaisePress(new Vector2(10f, 10f));

            Assert.That(fakeBoardView.ShownMarks, Is.Empty);
        }
    }
}
